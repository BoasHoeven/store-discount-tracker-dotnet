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
services.AddTransient<NextWeekDiscountJob>();
services.AddTransient<CurrentWeekDiscountJob>();
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

// Schedule NextWeekDiscountJob to run on Fridays at 6 PM
var nextWeekJob = JobBuilder.Create<NextWeekDiscountJob>()
    .WithIdentity("NextWeekDiscountJob", "Group1")
    .Build();

var nextWeekTrigger = TriggerBuilder.Create()
    .WithIdentity("NextWeekDiscountTrigger", "Group1")
    .WithCronSchedule("0 0 18 ? * FRI *") // run at 6 PM every Friday
    .ForJob(nextWeekJob)
    .Build();

await scheduler.ScheduleJob(nextWeekJob, nextWeekTrigger);

// Schedule CurrentWeekDiscountJob to run on Mondays at 9 PM
var currentWeekJob = JobBuilder.Create<CurrentWeekDiscountJob>()
    .WithIdentity("CurrentWeekDiscountJob", "Group1")
    .Build();

var currentWeekTrigger = TriggerBuilder.Create()
    .WithIdentity("CurrentWeekDiscountTrigger", "Group1")
    .WithCronSchedule("0 0 21 ? * MON *") // run at 9 PM every Monday
    .ForJob(currentWeekJob)
    .Build();

await scheduler.ScheduleJob(currentWeekJob, currentWeekTrigger);

// Stall indefinitely 
await Task.Delay(Timeout.Infinite);