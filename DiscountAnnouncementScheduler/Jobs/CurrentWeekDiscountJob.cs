using DiscountAnnouncementScheduler.Abstract;
using DiscountAnnouncementScheduler.Configuration;
using Microsoft.Extensions.Options;
using ProductMonitoringService.Services;
using Quartz;
using Telegram.Bot;

namespace DiscountAnnouncementScheduler.Jobs;

public class CurrentWeekDiscountJob : MessageJob
{
    public CurrentWeekDiscountJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
        : base(botClient, storeDiscountService, channelConfiguration)
    {
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        var currentWeekDiscounts = await StoreDiscountService.GetDiscountsForWeek(0);
        await SendDiscountsMessage(currentWeekDiscounts, "Deze week in de bonus:", "Er zijn geen producten in de bonus deze week");
    }
}
