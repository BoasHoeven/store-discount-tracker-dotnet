using Microsoft.Extensions.Logging;
using Scraper.Contracts;
using Scraper.Services;
using SharedServices.Extensions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Enums;

namespace TelegramBot.Services;

public sealed class UpdateHandler : IUpdateHandler
{
    private readonly Dictionary<long, ConversationState> chatStates = new();

    private readonly ITelegramBotClient botClient;
    private readonly ILogger<UpdateHandler> logger;
    private readonly ProductService productService;

    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger, ProductService productService)
    {
        ArgumentNullException.ThrowIfNull(botClient);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(productService);
        
        this.botClient = botClient;
        this.logger = logger;
        this.productService = productService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        var handler = update switch
        {
            { Message: { } message }             => BotOnMessageReceived(message, cancellationToken),
            { EditedMessage: { } message }       => BotOnMessageReceived(message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            _                                    => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handler;
    }
    
    private static string FormatProductsByStore(IEnumerable<IProduct> products)
    {
        var productGroups = products.GroupBy(p => p.StoreName);
        var storeProductStrings = new List<string>();
        foreach (var group in productGroups)
        {
            var productNames = group.Select(p => p.ToString()!.EscapeMarkdown());
            var storeProductsString = $"*{group.Key}*\n{string.Join("\n", productNames)}";
            storeProductStrings.Add(storeProductsString);
        }

        return string.Join("\n\n", storeProductStrings);
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Receive message type: {MessageType}", message.Type);

        if (message.Text is not { } messageText)
            return;

        chatStates.TryAdd(message.Chat.Id, ConversationState.Normal);

        var state = chatStates[message.Chat.Id];

        if (state != ConversationState.Normal)
        {
            await HandleConversationState(message, cancellationToken);
        }
        else
        {
            var action = messageText.Split(' ')[0] switch
            {
                "/list"   => ListProducts(botClient, message, cancellationToken),
                "/add"    => Add(botClient, message, cancellationToken),
                "/remove" => Remove(botClient, message, cancellationToken),
                _         => Usage(botClient, message, cancellationToken)
            };
            await action;
        }

        async Task<Message> ListProducts(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var products = productService.GetAllProducts();
            var formattedProducts = FormatProductsByStore(products);

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.IsNullOrEmpty(formattedProducts) ? "You have no products tracked yet".EscapeMarkdown() : formattedProducts,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }
        
        async Task<Message> Add(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            chatStates[message.Chat.Id] = ConversationState.WaitingForProductUrl;

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Send me the url of the product you want to add",
                cancellationToken: cancellationToken);
        }
        
        async Task<Message> Remove(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            chatStates[message.Chat.Id] = ConversationState.WaitingForRemoveProductName;

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Send me the name of the product you want to remove",
                cancellationToken: cancellationToken);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            const string usage = "Usage:\n" +
                                 "/list - display products\n" +
                                 "/add - add a product from a URL\n" +
                                 "/remove - remove a product by its name or a part of its name";

            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data!.StartsWith("remove_"))
        {
            var parts = callbackQuery.Data.Split('_', 3);
            if (parts.Length < 3) return; // Invalid callback data

            var productId = parts[1];
            var storeName = parts[2];

            var removedProduct = await productService.RemoveProduct(productId, storeName);
            
            var resultMessage = removedProduct != null 
                ? $"Product '{removedProduct}' has been successfully removed from {storeName}."
                : $"Failed to remove product with ID {productId} from {storeName}. It may not exist in the database";

            await botClient.SendTextMessageAsync(chatId: callbackQuery.Message!.Chat.Id,
                text: resultMessage,
                cancellationToken: cancellationToken);
        }
    }
    
    private async Task HandleConversationState(Message message, CancellationToken cancellationToken)
    {
        var state = chatStates[message.Chat.Id];
        
        switch(state)
        {
            case ConversationState.WaitingForProductUrl:
                var addProductMessage = await productService.AddProductFromMessage(message.Text!, message.From!.Id);
                
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: addProductMessage,
                    cancellationToken: cancellationToken,
                    parseMode: ParseMode.Markdown);

                chatStates[message.Chat.Id] = ConversationState.Normal;
                break;
            case ConversationState.WaitingForRemoveProductName:
                var productMatches = productService.GetProductsByName(message.Text!).Take(2).ToList();
            
                if (!productMatches.Any())
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "No product(s) found matching the name you provided",
                        cancellationToken: cancellationToken,
                        parseMode: ParseMode.Markdown);
                    break;
                }

                var inlineKeyboard = new InlineKeyboardMarkup(productMatches.Select(product =>
                {
                    var shortStoreName = productService.GetStoreShortNameFromProduct(product);

                    return InlineKeyboardButton.WithCallbackData($"{shortStoreName}: {product}",
                        $"remove_{product.Id}_{product.StoreName}");
                }).ToArray());
                
                await botClient.SendTextMessageAsync(  
                    chatId: message.Chat.Id,
                    text: "Choose a product to remove:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
                
                chatStates[message.Chat.Id] = ConversationState.Normal;
                break;
            case ConversationState.Normal:
            default:
                break;
        }
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}