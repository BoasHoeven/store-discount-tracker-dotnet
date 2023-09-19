namespace DiscountAnnouncementScheduler.Configuration;

public sealed class TelegramChannelConfiguration
{
    public const string Configuration = "TelegramChannelConfiguration";
    public long ChannelId { get; init; }
    public long? NotificationChannelId { get; init; }
}