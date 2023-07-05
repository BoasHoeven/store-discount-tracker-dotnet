using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Scraper.Extensions;
using Telegram.Bot;
using TelegramBot.Configuration;
using TelegramBot.Configuration.Validations;
using TelegramBot.Extensions;
using TelegramBot.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScraperServices();

        services.AddOptions<BotConfiguration>()
            .Bind(context.Configuration.GetSection(BotConfiguration.Configuration))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<BotConfiguration>, BotConfigurationValidation>();

        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var botConfig = sp.GetRequiredService<IOptions<BotConfiguration>>().Value;
                TelegramBotClientOptions options = new(botConfig.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    })

    .Build();

await host.RunAsync();
