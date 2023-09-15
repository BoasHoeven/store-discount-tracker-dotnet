using ProductMonitoringService.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAPI.Abstract;
using TelegramAPI.Enums;

namespace TelegramAPI.Conversations;

public sealed class WaitingForProductUrlStrategy : IConversationStrategy
{
    private readonly ProductService productService;

    public WaitingForProductUrlStrategy(ProductService productService)
    {
        this.productService = productService;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken)
    {
        var addProductMessage = await productService.AddProductFromMessage(message.Text!, message.From!.Id);

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: addProductMessage,
            cancellationToken: cancellationToken,
            parseMode: ParseMode.Markdown);

        setStateCallback(message.Chat.Id, ConversationState.Normal);
    }
}
