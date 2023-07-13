using MessageScheduler.Configuration;
using MessageScheduler.Factory;
using MessageScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Scraper.Extensions;
using SharedServices;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

services.AddSingleton<IConfiguration>(configuration);

services.AddTelegramHttpClient(configuration);

services.AddOptions<TelegramChannelConfiguration>()
    .Bind(configuration.GetSection(TelegramChannelConfiguration.Configuration));

services.AddScraperServices();
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


/*
    * * * * * ? *
    | | | | | | |
    | | | | | | +-- Year              (range: 1970-2099)
    | | | | | +---- Day of the Week   (range: 1-7, 1 standing for Sunday)
    | | | | +------ Month of the Year (range: 1-12)
    | | | +-------- Day of the Month  (range: 1-31)
    | | +---------- Hour              (range: 0-23)
    | +------------ Minute            (range: 0-59)
    +-------------- Second            (range: 0-59)
*/
var trigger = TriggerBuilder.Create()
    .WithIdentity("MessageTrigger", "Group1")
    .WithCronSchedule("0 0 12 ? * MON *")  // run at 12 o'clock every Monday
    .ForJob(job)
    .Build();

await scheduler.ScheduleJob(job, trigger);
await Task.Delay(Timeout.Infinite);