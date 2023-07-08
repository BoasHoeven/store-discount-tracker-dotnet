using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedServices.Configuration;
using SharedServices.Configuration.Validations;
using Telegram.Bot;

namespace SharedServices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BotConfiguration>()
            .Bind(configuration.GetSection(BotConfiguration.Configuration))
            .ValidateOnStart();
        
        services.AddSingleton<IValidateOptions<BotConfiguration>, BotConfigurationValidation>();

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
        
        return services;
    }
}