using System.Text.Json;
using Scraper.ConcreteClasses;
using Scraper.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Enums;

namespace TelegramBot.Services;

public sealed class ConversationService
{
    private readonly Dictionary<long, ConversationState> chatStates = new();
    private readonly ProductService productService;
        
    public ConversationService(ProductService productService)
    {
        ArgumentNullException.ThrowIfNull(productService);
        this.productService = productService;
    }

    public bool IsInConversation(long chatId)
    {
        return chatStates.ContainsKey(chatId) && chatStates[chatId] != ConversationState.Normal;
    }

    public async Task HandleConversationStateAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (!chatStates.TryGetValue(message.Chat.Id, out var state))
        {
            chatStates[message.Chat.Id] = ConversationState.Normal;
            return;
        }

        switch (state)
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
                // Reset conversation state
                chatStates[message.Chat.Id] = ConversationState.Normal;

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

                break;
            case ConversationState.WaitingForImportFile:
            {
                if (message.Document == null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Please send me the file containing the products.",
                        cancellationToken: cancellationToken);
                    return;
                }

                // Download the file sent by the user
                var file = await botClient.GetFileAsync(message.Document.FileId, cancellationToken: cancellationToken);
                using var stream = new MemoryStream();
                await botClient.DownloadFileAsync(file.FilePath!, stream, cancellationToken);

                // Move the stream position to the beginning
                stream.Seek(0, SeekOrigin.Begin);

                try
                {
                    // Deserialize the content of the file into product objects
                    var jsonString = await new StreamReader(stream).ReadToEndAsync(cancellationToken);
                    var products = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonString);

                    var successfullyImported = await productService.ImportProducts(products!);

                    if (!successfullyImported)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Something went wrong importing the products.",
                            cancellationToken: cancellationToken);
                        return;
                    }
                    
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Products have been imported successfully.",
                        cancellationToken: cancellationToken);
                }
                catch (JsonException)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "The file format is incorrect. Please send a valid products JSON file.",
                        cancellationToken: cancellationToken);
                }

                chatStates[message.Chat.Id] = ConversationState.Normal;
                break;
            }
            case ConversationState.Normal:
            default:
                break;
        }
    }

    public void SetState(long chatId, ConversationState newState)
    {
        chatStates[chatId] = newState;
    }
}