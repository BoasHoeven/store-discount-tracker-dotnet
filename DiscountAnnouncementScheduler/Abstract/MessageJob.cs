using System.Globalization;
using System.Net;
using System.Text;
using DiscountAnnouncementScheduler.Configuration;
using Microsoft.Extensions.Options;
using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Services;
using Quartz;
using SharedUtilities.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DiscountAnnouncementScheduler.Abstract;

public abstract class MessageJob : IJob
{
    private readonly StoreDiscountService storeDiscountService;

    private readonly ITelegramBotClient botClient;
    private readonly TelegramChannelConfiguration telegramChannelConfiguration;
    private static readonly CultureInfo Culture = new("nl-NL");

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

        var message = new StringBuilder($"{prefix}\n\n");
        foreach (var storeGroup in groupedByStore)
        {
            var storeName = storeGroup.Key;
            var storeDiscounts = storeGroup.ToList();

            message.AppendLine($"<b>{storeName}</b>");
            message.AppendLine();

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
                    message.AppendLine($"- {discount.Product}");
                    foreach (var detail in discountDetails)
                    {
                        message.AppendLine($"  • {detail.ToLower().Trim()}");
                    }
                }
                else
                {
                    message.AppendLine($"- {discount.Product}\n  • was <s>€{discount.OldPrice}</s> nu €{discount.NewPrice}!");
                }

                message.AppendLine();
            }
        }

        await botClient.SendTextMessageAsync(telegramChannelConfiguration.ChannelId, message.ToString(), parseMode: ParseMode.Html);
    }

    private static string FormatDateString(DateTime startDate, DateTime endDate)
    {
        var utcNow = DateTime.UtcNow;
        var startDateWeekDiff = GetWeekDiff(startDate, utcNow);

        if (startDateWeekDiff < 0)
        {
            return $"t/m {endDate.ToString("dddd", Culture)} {endDate.ToString("dd MMMM yyyy", Culture)}";
        }

        return $"{FormatStartDate(startDate)} t/m {FormatEndDate(endDate)}";
    }

    private static string FormatStartDate(DateTime date)
    {
        return date.ToString("dddd", Culture);
    }

    private static string FormatEndDate(DateTime endDate)
    {
        var utcNow = DateTime.UtcNow;
        var weekDiff = GetWeekDiff(endDate, utcNow);

        return weekDiff switch
        {
            0 => endDate.ToString("dddd", Culture),
            1 => "volgende week " + endDate.ToString("dddd", Culture),
            _ => endDate.ToString("dd MMMM yyyy", Culture)
        };
    }

    private static int GetWeekDiff(DateTime dateTimeA, DateTime dateTimeB)
    {
        return dateTimeA.GetWeekNumber() - dateTimeB.GetWeekNumber();
    }

    private async Task NotifyOfFailedRequests(IEnumerable<ProductDiscountResponse> productDiscountResponses)
    {
        if (!telegramChannelConfiguration.NotificationChannelId.HasValue)
            return;

        var failedRequests = productDiscountResponses.Where(x => x.ProductResponse.StatusCode != HttpStatusCode.OK);

        if (failedRequests.Any())
        {
            var totalRequests = productDiscountResponses.Count();
            var totalFailedRequests = failedRequests.Count();

            var groupedByStore = failedRequests.GroupBy(x => x.ProductResponse.Product.StoreName);
            var sb = new StringBuilder($"Total Requests: {totalRequests}, Total Failed: {totalFailedRequests}\n\n");

            foreach (var storeGroup in groupedByStore)
            {
                sb.AppendLine($"Store: {storeGroup.Key}");

                var groupedByStatusCode = storeGroup.GroupBy(x => x.ProductResponse.StatusCode);
                foreach (var statusCodeGroup in groupedByStatusCode)
                {
                    sb.AppendLine($"- Status Code: {statusCodeGroup.Key}, Count: {statusCodeGroup.Count()}");

                    foreach (var productDiscountResponse in statusCodeGroup.Take(5))
                    {
                        var product = productDiscountResponse.ProductResponse.Product;
                        sb.AppendLine($"  - Product ID: {product.Id}, Name: {product.Name}");
                    }
                    if (statusCodeGroup.Count() > 5)
                    {
                        sb.AppendLine($"  ...and {statusCodeGroup.Count() - 5} more.");
                    }
                }
                sb.AppendLine();
            }

            await botClient.SendTextMessageAsync(telegramChannelConfiguration.NotificationChannelId, sb.ToString());
        }
    }

    protected async Task<IEnumerable<ProductDiscount>> GetDiscountsForWeek(int weekOffset)
    {
        // Get all discounts
        var productDiscountResponses = await storeDiscountService.GetDiscounts();

        await NotifyOfFailedRequests(productDiscountResponses);

        var discounts = productDiscountResponses
            .Where(x => x.ProductDiscount is not null)
            .Select(x => x.ProductDiscount);

        // Get the current week of the year
        var currentWeek = DateTime.UtcNow.GetWeekNumber();

        // Filter the discounts based on the weekOffset
        var filteredDiscounts = discounts.Where(discount => discount!.StartDate.GetWeekNumber() == currentWeek + weekOffset ||
                                                            discount.EndDate.GetWeekNumber() == currentWeek + weekOffset);

        return filteredDiscounts!;
    }
}