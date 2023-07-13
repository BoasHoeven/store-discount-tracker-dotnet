using MessageScheduler.Abstract;
using MessageScheduler.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using Scraper.Services;
using Telegram.Bot;

namespace MessageScheduler.Jobs;

public class NextWeekDiscountJob : MessageJob
{
    public NextWeekDiscountJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
        : base(botClient, storeDiscountService, channelConfiguration)
    {
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        var nextWeekDiscounts = await storeDiscountService.GetDiscountsForWeek(1);
        await SendDiscountsMessage(nextWeekDiscounts, "Volgende week in de bonus:", "Er zijn geen producten in de bonus volgende week.");
    }
}
