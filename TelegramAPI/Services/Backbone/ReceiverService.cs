using Telegram.Bot;
using TelegramAPI.Abstract;

namespace TelegramAPI.Services.Backbone;

public sealed class ReceiverService : ReceiverServiceBase<UpdateHandler>
{
    public ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler) : base(botClient, updateHandler)
    {
    }
}