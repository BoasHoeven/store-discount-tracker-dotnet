using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductMonitoringService.Extensions;
using ProductTelegramAPI.Services;
using ProductTelegramAPI.Services.Backbone;
using SharedUtilities;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScraperServices();
        services.AddTelegramHttpClient(context.Configuration);
        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
        services.AddScoped<CallbackService>();
        services.AddScoped<ConversationService>();
        services.AddScoped<CommandService>();
    }).Build();

await host.RunAsync();
