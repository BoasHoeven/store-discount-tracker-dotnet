using MessageScheduler.Configuration;
using MessageScheduler.Factory;
using MessageScheduler.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var scheduler = serviceProvider.GetRequiredService<IScheduler>();

var logger = serviceProvider.GetService<ILogger<Program>>(); // Getting the logger

await scheduler.Start();

scheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();

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

// Schedule CurrentWeekDiscountJob to run on Tuesdays at 9 AM
var currentWeekJob = JobBuilder.Create<CurrentWeekDiscountJob>()
    .WithIdentity("CurrentWeekDiscountJob", "Group1")
    .Build();

var currentWeekTrigger = TriggerBuilder.Create()
    .WithIdentity("CurrentWeekDiscountTrigger", "Group1")
    .WithCronSchedule("0 0 9 ? * TUE *") // run at 9 AM every Tuesday
    .ForJob(currentWeekJob)
    .Build();

await scheduler.ScheduleJob(currentWeekJob, currentWeekTrigger);

var nextWeekTriggerInfo = await scheduler.GetTrigger(nextWeekTrigger.Key);
var nextWeekJobFireTime = nextWeekTriggerInfo!.GetNextFireTimeUtc();
logger.LogInformation("NextWeekDiscountJob is scheduled to run next at: {NextWeekJobFireTime}", nextWeekJobFireTime);

var currentWeekTriggerInfo = await scheduler.GetTrigger(currentWeekTrigger.Key);
var currentWeekJobFireTime = currentWeekTriggerInfo!.GetNextFireTimeUtc();
logger.LogInformation("CurrentWeekDiscountJob is scheduled to run next at: {CurrentWeekJobFireTime}", currentWeekJobFireTime);


// Stall indefinitely 
await Task.Delay(Timeout.Infinite);
