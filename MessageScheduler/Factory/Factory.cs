using Quartz;
using Quartz.Spi;

namespace MessageScheduler.Factory;

public class JobFactory : IJobFactory
{
    private readonly IServiceProvider container;

    public JobFactory(IServiceProvider container)
    {
        this.container = container;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return container.GetService(bundle.JobDetail.JobType) as IJob;
    }

    public void ReturnJob(IJob job)
    {
        (job as IDisposable)?.Dispose();
    }
}
