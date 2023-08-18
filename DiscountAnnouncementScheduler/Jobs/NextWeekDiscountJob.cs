using DiscountAnnouncementScheduler.Abstract;
using DiscountAnnouncementScheduler.Configuration;
using Microsoft.Extensions.Options;
using ProductMonitoringService.Services;
using Quartz;
using Telegram.Bot;

namespace DiscountAnnouncementScheduler.Jobs;

public class NextWeekDiscountJob : MessageJob
{
    public NextWeekDiscountJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
        : base(botClient, storeDiscountService, channelConfiguration)
    {
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        var nextWeekDiscounts = await StoreDiscountService.GetDiscountsForWeek(1);
        await SendDiscountsMessage(nextWeekDiscounts, "Volgende week in de bonus:", "Er zijn geen producten in de bonus volgende week");
    }
}
