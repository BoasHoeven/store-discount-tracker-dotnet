namespace TelegramBot.Configuration;

public sealed class BotConfiguration
{
    public static readonly string Configuration = "BotConfiguration";

    public string BotToken { get; set; } = "";
}
