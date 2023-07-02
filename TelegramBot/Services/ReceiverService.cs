using Telegram.Bot;
using TelegramBot.Abstract;

namespace TelegramBot.Services;

public class ReceiverService : ReceiverServiceBase<UpdateHandler>
{
    public ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler) : base(botClient, updateHandler)
    {
    }
}