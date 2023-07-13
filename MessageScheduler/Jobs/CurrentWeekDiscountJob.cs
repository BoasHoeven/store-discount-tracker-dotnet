using MessageScheduler.Abstract;
using MessageScheduler.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using Scraper.Services;
using Telegram.Bot;

namespace MessageScheduler.Jobs;

public class CurrentWeekDiscountJob : MessageJob
{
    public CurrentWeekDiscountJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
        : base(botClient, storeDiscountService, channelConfiguration)
    {
    }
    
    public override async Task Execute(IJobExecutionContext context)
    {
        var currentWeekDiscounts = await storeDiscountService.GetDiscountsForWeek(0);
        await SendDiscountsMessage(currentWeekDiscounts, "Deze week in de bonus:", "Er zijn geen producten in de bonus deze week.");
    }
}
