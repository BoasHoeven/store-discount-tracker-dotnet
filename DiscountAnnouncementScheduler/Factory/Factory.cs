using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace DiscountAnnouncementScheduler.Factory;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider serviceProvider;

    public JobFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return (serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob)!;
    }

    public void ReturnJob(IJob job)
    {
        (job as IDisposable)?.Dispose();
    }
}
