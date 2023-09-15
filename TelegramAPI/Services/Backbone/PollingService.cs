using Microsoft.Extensions.Logging;
using TelegramAPI.Abstract;

namespace TelegramAPI.Services.Backbone;

public sealed class PollingService : PollingServiceBase<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger) : base(serviceProvider, logger)
    {
    }
}