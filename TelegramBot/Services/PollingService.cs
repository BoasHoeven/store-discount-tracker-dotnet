using Microsoft.Extensions.Logging;
using TelegramBot.Abstract;

namespace TelegramBot.Services;

public class PollingService : PollingServiceBase<ReceiverService>
{
    public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger) : base(serviceProvider, logger)
    {
    }
}