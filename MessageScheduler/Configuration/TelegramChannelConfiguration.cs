namespace MessageScheduler.Configuration;

public sealed class TelegramChannelConfiguration
{
    public const string Configuration = "TelegramChannelConfiguration";
    public long ChannelId { get; init; }
}