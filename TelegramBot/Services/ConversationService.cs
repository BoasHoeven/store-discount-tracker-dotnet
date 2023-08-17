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
        if (botClient == null)
        {
            throw new ArgumentNullException(nameof(botClient));
        }

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

    public void SetState(long chatId, ConversationState newState)
    {
        chatStates[chatId] = newState;
    }
}