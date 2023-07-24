using System.Globalization;
using System.Text;
using MessageScheduler.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using Scraper.ConcreteClasses;
using Scraper.Services;
using SharedServices.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MessageScheduler.Abstract;

public abstract class MessageJob : IJob
{
    protected readonly StoreDiscountService StoreDiscountService;

    private readonly ITelegramBotClient botClient;
    private readonly TelegramChannelConfiguration telegramChannelConfiguration;

    protected MessageJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
    {
        ArgumentNullException.ThrowIfNull(botClient);
        ArgumentNullException.ThrowIfNull(storeDiscountService);
        ArgumentNullException.ThrowIfNull(channelConfiguration);

        telegramChannelConfiguration = channelConfiguration.Value;
        this.botClient = botClient;
        StoreDiscountService = storeDiscountService;
    }

    public abstract Task Execute(IJobExecutionContext context);

    protected async Task SendDiscountsMessage(IEnumerable<ProductDiscount> discounts, string prefix, string noDiscountsMessage)
    {
        if (!discounts.Any())
        {
            await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, noDiscountsMessage, parseMode: ParseMode.Html);
            return;
        }

        var groupedByStore = discounts.GroupBy(d => d.Product.StoreName);

        var allMessages = new StringBuilder($"{prefix}\n\n");

        foreach (var storeGroup in groupedByStore)
        {
            var storeName = storeGroup.Key;
            var storeDiscounts = storeGroup.ToList();

            var message = new StringBuilder();
            message.AppendLine($"<b>{storeName}</b>");

            var weekStart = storeDiscounts.Min(d => d.StartDate);
            var weekEnd = storeDiscounts.Max(d => d.EndDate);
            var isFullWeek = weekStart.DayOfWeek == DayOfWeek.Monday && weekEnd.DayOfWeek == DayOfWeek.Sunday;
            
            if (!isFullWeek)
            {
                var weekDayRange = FormatDateString(weekStart, weekEnd);
                message.AppendLine($"<b>Geldig</b>: {weekDayRange}");
            }
                
            foreach (var discount in storeDiscounts)
            {
                if (!string.IsNullOrEmpty(discount.DiscountMessage))
                {
                    var discountDetails = discount.DiscountMessage.Split('&');
                    message.AppendLine($"- {discount.Product} (€{discount.Product.Price})");
                    foreach (var detail in discountDetails)
                    {
                        message.AppendLine($"  - {detail.Trim()}");
                    }
                }
                else
                {
                    message.AppendLine($"- {discount.Product} was <s>€{discount.OldPrice}</s> nu €{discount.NewPrice}!");
                }
            }

            allMessages.AppendLine(message.ToString());
        }
        
        await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, allMessages.ToString(), parseMode: ParseMode.Html);
    }

    private static string FormatDateString(DateTime startDate, DateTime endDate)
    {
        var utcNow = DateTime.UtcNow;
        var startDateWeekDiff = GetWeekDiff(startDate, utcNow);

        if (startDateWeekDiff < 0)
        {
            return $"t/m {endDate.ToString("dddd", new CultureInfo("nl-NL"))} {endDate.ToString("dd MMMM yyyy", new CultureInfo("nl-NL"))}";
        }

        return $"{FormatStartDate(startDate)} t/m {FormatEndDate(endDate)}";
    }

    private static string FormatStartDate(DateTime date)
    {
        return date.ToString("dddd", new CultureInfo("nl-NL"));
    }

    private static string FormatEndDate(DateTime endDate)
    {
        var utcNow = DateTime.UtcNow;
        var weekDiff = GetWeekDiff(endDate, utcNow);

        return weekDiff switch
        {
            0 => endDate.ToString("dddd", new CultureInfo("nl-NL")),
            1 => "volgende week " + endDate.ToString("dddd", new CultureInfo("nl-NL")),
            _ => endDate.ToString("dd MMMM yyyy", new CultureInfo("nl-NL"))
        };
    }

    private static int GetWeekDiff(DateTime dateTimeA, DateTime dateTimeB)
    {
        return dateTimeA.GetWeekNumber() - dateTimeB.GetWeekNumber();
    }
}