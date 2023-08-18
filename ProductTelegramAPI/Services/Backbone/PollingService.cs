using Microsoft.Extensions.Logging;
using ProductTelegramAPI.Abstract;

namespace ProductTelegramAPI.Services.Backbone;

public sealed class PollingService : PollingServiceBase<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger) : base(serviceProvider, logger)
    {
    }
}