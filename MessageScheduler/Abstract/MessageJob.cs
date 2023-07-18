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

            var message = new StringBuilder($"{storeName}:\n");

            var groupedByWeek = storeDiscounts.GroupBy(d => d.StartDate.GetWeekNumber());
            foreach (var weekGroup in groupedByWeek)
            {
                var weekDiscounts = weekGroup.ToList();
                var weekStart = weekDiscounts.Min(d => d.StartDate);
                var weekEnd = weekDiscounts.Max(d => d.EndDate);

                var weekDayRange = $"{FormatStartDate(weekStart)} t/m {FormatEndDate(weekEnd)}";

                message.AppendLine($"    {weekDayRange}:");

                foreach (var discountMessage in weekDiscounts.Select(CreateDiscountMessage))
                {
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
        return Math.Abs(dateTimeA.GetWeekNumber() - dateTimeB.GetWeekNumber());
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
}