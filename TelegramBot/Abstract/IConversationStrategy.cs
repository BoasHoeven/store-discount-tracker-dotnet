using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Enums;

namespace TelegramBot.Abstract;

public interface IConversationStrategy
{
    Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken);
}