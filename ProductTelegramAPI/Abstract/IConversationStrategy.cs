using ProductTelegramAPI.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ProductTelegramAPI.Abstract;

public interface IConversationStrategy
{
    Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken);
}