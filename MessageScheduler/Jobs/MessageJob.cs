using Telegram.Bot.Types;

namespace MessageScheduler.Jobs;

using Quartz;
using System.Threading.Tasks;
using Telegram.Bot;

public class MessageJob : IJob
{
    private readonly ITelegramBotClient botClient;

    public MessageJob(ITelegramBotClient botClient)
    {
        this.botClient = botClient;
    }
 
    public Task Execute(IJobExecutionContext context)
    {
        var chatId = new ChatId(-1001676414343);
        return botClient.SendTextMessageAsync(chatId, "Hello, this is scheduled message!");
    }
}
