using System.Globalization;
using System.Text;
using MessageScheduler.Configuration;
using Microsoft.Extensions.Options;
using Scraper.Services;
using Telegram.Bot.Types.Enums;

namespace MessageScheduler.Jobs;

using Quartz;
using System.Threading.Tasks;
using Telegram.Bot;

public class MessageJob : IJob
{
    private readonly ITelegramBotClient botClient;
    private readonly StoreDiscountService storeDiscountService;
    private readonly TelegramChannelConfiguration telegramChannelConfiguration;

    public MessageJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
    {
        ArgumentNullException.ThrowIfNull(botClient);
        ArgumentNullException.ThrowIfNull(storeDiscountService);
        ArgumentNullException.ThrowIfNull(channelConfiguration);

        telegramChannelConfiguration = channelConfiguration.Value;
        this.botClient = botClient;
        this.storeDiscountService = storeDiscountService;
    }
 
    public async Task Execute(IJobExecutionContext context)
    {
        var discounts = await storeDiscountService.GetDiscounts();

        if (!discounts.Any())
        {
            await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, "Er zijn geen producten in de bonus deze week.", parseMode: ParseMode.Html);
            return;
        }

        var groupedByStore = discounts.GroupBy(d => d.Product.StoreName);

        foreach (var storeGroup in groupedByStore)
        {
            var storeName = storeGroup.Key;
            var storeDiscounts = storeGroup.ToList();

            var message = new StringBuilder($"<b>{storeName}</b>\n");

            var groupedByWeek = storeDiscounts.GroupBy(d => GetWeekOfYear(d.StartDate));
            foreach (var weekGroup in groupedByWeek)
            {
                var weekDiscounts = weekGroup.ToList();
                var weekStart = weekDiscounts.Min(d => d.StartDate);
                var weekEnd = weekDiscounts.Max(d => d.EndDate);

                var weekDayRange = $"{weekStart.ToString("dddd", new CultureInfo("nl-NL"))} t/m {weekEnd.ToString("dddd", new CultureInfo("nl-NL"))}";

                if (GetWeekOfYear(DateTime.Now) == weekGroup.Key)
                {
                    message.AppendLine($"    <i>{weekDayRange}</i>");
                }
                else if (GetWeekOfYear(DateTime.Now) + 1 == weekGroup.Key)
                {
                     message.AppendLine($"    <i>Volgende week {weekDayRange}</i>");
                }
                else
                {
                    weekDayRange = $"{weekStart:dd/MM} t/m {weekEnd:dd/MM}";
                    message.AppendLine($"    <i>Van {weekDayRange}</i>");
                }

                foreach (var discount in weekDiscounts)
                {
                    message.AppendLine($"    - {discount.Product} van <s>€{discount.OldPrice}</s> nu voor €{discount.NewPrice}!");
                }

                message.AppendLine();
            }

            await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, message.ToString(), parseMode: ParseMode.Html);
        }
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var dfi = DateTimeFormatInfo.CurrentInfo;
        var cal = dfi.Calendar;
        return cal.GetWeekOfYear(date, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
    }
}
