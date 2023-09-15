using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramAPI.Enums;

namespace TelegramAPI.Abstract;

public interface IConversationStrategy
{
    Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken);
}