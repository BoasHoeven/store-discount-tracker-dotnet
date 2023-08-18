using ProductTelegramAPI.Abstract;
using Telegram.Bot;

namespace ProductTelegramAPI.Services.Backbone;

public sealed class ReceiverService : ReceiverServiceBase<UpdateHandler>
{
    public ReceiverService(ITelegramBotClient botClient, UpdateHandler updateHandler) : base(botClient, updateHandler)
    {
    }
}