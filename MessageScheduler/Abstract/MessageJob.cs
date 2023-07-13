using System.Globalization;
using System.Text;
using MessageScheduler.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using Scraper.ConcreteClasses;
using Scraper.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MessageScheduler.Abstract;

public abstract class MessageJob : IJob
{
    private readonly ITelegramBotClient botClient;
    protected readonly StoreDiscountService storeDiscountService;
    private readonly TelegramChannelConfiguration telegramChannelConfiguration;

    protected MessageJob(ITelegramBotClient botClient, StoreDiscountService storeDiscountService, IOptions<TelegramChannelConfiguration> channelConfiguration)
    {
        ArgumentNullException.ThrowIfNull(botClient);
        ArgumentNullException.ThrowIfNull(storeDiscountService);
        ArgumentNullException.ThrowIfNull(channelConfiguration);

        telegramChannelConfiguration = channelConfiguration.Value;
        this.botClient = botClient;
        this.storeDiscountService = storeDiscountService;
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

            var message = new StringBuilder($"{storeName}:\n");

            var groupedByWeek = storeDiscounts.GroupBy(d => GetWeekOfYear(d.StartDate));
            foreach (var weekGroup in groupedByWeek)
            {
                var weekDiscounts = weekGroup.ToList();
                var weekStart = weekDiscounts.Min(d => d.StartDate);
                var weekEnd = weekDiscounts.Max(d => d.EndDate);

                var weekDayRange = $"{FormatStartDate(weekStart)} t/m {FormatEndDate(weekEnd)}";

                message.AppendLine($"    {weekDayRange}:");

                foreach (var discount in weekDiscounts)
                {
                    var discountMessage = CreateDiscountMessage(discount);
                    message.AppendLine($"        {discountMessage}");
                }

                allMessages.AppendLine(message.ToString());
            }
        }
        
        await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, allMessages.ToString(), parseMode: ParseMode.Html);
    }
    
    private static string FormatStartDate(DateTime date)
    {
        return date.ToString("dddd", new CultureInfo("nl-NL"));
    }

    private static string FormatEndDate(DateTime date)
    {
        if (GetWeekDiff(date, DateTime.Now.Date) == 0)
            return date.ToString("dddd", new CultureInfo("nl-NL"));

        if (GetWeekDiff(date, DateTime.Now.Date) == 1) 
            return "volgende week " + date.ToString("dddd", new CultureInfo("nl-NL"));

        return date.ToString("dd MMMM yyyy", new CultureInfo("nl-NL"));
    }

    private static int GetWeekDiff(DateTime date1, DateTime date2)
    {
        return Math.Abs(GetWeekNumber(date1) - GetWeekNumber(date2));
    }
    
    private static int GetWeekNumber(DateTime date)
    {
        var culture = new CultureInfo("nl-NL");
        return culture.Calendar.GetWeekOfYear(
            date,
            culture.DateTimeFormat.CalendarWeekRule,
            culture.DateTimeFormat.FirstDayOfWeek);
    }

    private static string CreateDiscountMessage(ProductDiscount productDiscount)
    {
        string discountMessage;
        if (productDiscount.DiscountMessage != string.Empty)
            discountMessage = $"{productDiscount.Product} {productDiscount.DiscountMessage}";
        else
            discountMessage = $"{productDiscount.Product} was <s>€{productDiscount.OldPrice}</s> nu €{productDiscount.NewPrice}!";

        return discountMessage;
    }
    
    private static int GetWeekOfYear(DateTime date)
    {
        var dateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo;
        var calendar = dateTimeFormatInfo.Calendar;
        return calendar.GetWeekOfYear(date, dateTimeFormatInfo.CalendarWeekRule, dateTimeFormatInfo.FirstDayOfWeek);
    }
}