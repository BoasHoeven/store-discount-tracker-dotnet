using Microsoft.Extensions.Logging;
using Polly;
using Polly.RateLimit;

namespace Scraper.Policies;

public class AlbertHeijnRateLimitPolicy : DelegatingHandler
{
    private readonly ILogger<AlbertHeijnRateLimitPolicy> logger;

    public AlbertHeijnRateLimitPolicy(ILogger<AlbertHeijnRateLimitPolicy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
    }
    private const int ApiCallsPerMinute = 60;
    
    private static readonly IAsyncPolicy RateLimitPolicy = 
        Policy.RateLimitAsync(ApiCallsPerMinute, TimeSpan.FromMinutes(1), ApiCallsPerMinute);

    private static readonly Random Jitter = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const int maxRetryAttempts = 10;
        const double initialBackoffDelayInSeconds = 60;

        for (var i = 0; i <= maxRetryAttempts; i++)
        {
            try
            {
                return await RateLimitPolicy.ExecuteAsync(async token =>
                    await base.SendAsync(request, token), cancellationToken);
            }
            catch (RateLimitRejectedException ex)
            {
                logger.LogInformation("Reached the rate limit of {CallsPerMinute} per minute, will retry in {RetryAfter}", ApiCallsPerMinute, ex.RetryAfter);
                if (i >= maxRetryAttempts)
                    throw;
                
                var delayInSeconds = Math.Pow(2, i) * initialBackoffDelayInSeconds;
                delayInSeconds *= Jitter.NextDouble() * 0.2 + 0.9;

                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken);
                logger.LogInformation("Finished waiting on the rate limit");
            }
        }

        throw new Exception("Rate limit exceeded after maximum retry attempts");
    }
}