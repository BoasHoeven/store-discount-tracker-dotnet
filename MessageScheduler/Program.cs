using MessageScheduler.Factory;
using MessageScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TelegramBot.Extensions;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

services.AddSingleton<IConfiguration>(configuration);

services.AddTelegramHttpClient(configuration);

services.AddSingleton<IJobFactory, JobFactory>();
services.AddTransient<MessageJob>();
services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
services.AddSingleton(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    return schedulerFactory.GetScheduler().Result;
});

var serviceProvider = services.BuildServiceProvider();
var scheduler = serviceProvider.GetRequiredService<IScheduler>();
await scheduler.Start();

scheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();

var job = JobBuilder.Create<MessageJob>()
    .WithIdentity("MessageJob", "Group1")
    .Build();

var trigger = TriggerBuilder.Create()
    .WithIdentity("MessageTrigger", "Group1")
    .WithCronSchedule("0 0/1 * 1/1 * ? *") // this runs every minute
    .ForJob(job)
    .Build();

await scheduler.ScheduleJob(job, trigger);
await Task.Delay(Timeout.Infinite);