using Scraper.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Abstract;
using TelegramBot.Enums;

namespace TelegramBot.Conversations;

public class WaitingForRemoveProductNameStrategy : IConversationStrategy
{
    private readonly ProductService productService;

    public WaitingForRemoveProductNameStrategy(ProductService productService)
    {
        this.productService = productService;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken)
    {
        setStateCallback(message.Chat.Id, ConversationState.Normal);

        var productMatches = productService.GetProductsByName(message.Text!).Take(2).ToList();

        if (!productMatches.Any())
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "No product(s) found matching the name you provided",
                cancellationToken: cancellationToken,
                parseMode: ParseMode.Markdown);
            return;
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
    }
}