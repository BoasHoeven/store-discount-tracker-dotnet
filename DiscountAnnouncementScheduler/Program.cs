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
using SharedUtilities;

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

await scheduler.Start();

scheduler.JobFactory = serviceProvider.GetRequiredService<IJobFactory>();

// Schedule NextWeekDiscountJob to run on Fridays at 6 PM
var nextWeekJob = JobBuilder.Create<NextWeekDiscountJob>()
    .WithIdentity("NextWeekDiscountJob", "Group1")
    .Build();

var nextWeekTrigger = TriggerBuilder.Create()
    .WithIdentity("NextWeekDiscountTrigger")
    .WithCronSchedule("0 0 18 ? * FRI *") // run at 6 PM every Friday
    .ForJob(nextWeekJob)
    .Build();

await scheduler.ScheduleJob(nextWeekJob, nextWeekTrigger);

// Schedule CurrentWeekDiscountJob to run on Tuesdays at 9 AM
var currentWeekJob = JobBuilder.Create<CurrentWeekDiscountJob>()
    .WithIdentity("CurrentWeekDiscountJob")
    .Build();

var currentWeekTrigger = TriggerBuilder.Create()
    .WithIdentity("CurrentWeekDiscountTrigger", "Group1")
    .WithCronSchedule("0 0 8 ? * MON *") // run at 8 AM every Monday
    .ForJob(currentWeekJob)
    .Build();

await scheduler.ScheduleJob(currentWeekJob, currentWeekTrigger);

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var dutchTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

var nextWeekTriggerInfo = await scheduler.GetTrigger(nextWeekTrigger.Key);
var nextWeekJobFireTimeUtc = nextWeekTriggerInfo!.GetNextFireTimeUtc();
var nextWeekJobFireTime = TimeZoneInfo.ConvertTimeFromUtc(nextWeekJobFireTimeUtc!.Value.DateTime, dutchTimeZone);
logger.LogInformation("NextWeekDiscountJob is scheduled to run next at: {NextWeekJobFireTime}", nextWeekJobFireTime);

var currentWeekTriggerInfo = await scheduler.GetTrigger(currentWeekTrigger.Key);
var currentWeekJobFireTimeUtc = currentWeekTriggerInfo!.GetNextFireTimeUtc();
var currentWeekJobFireTime = TimeZoneInfo.ConvertTimeFromUtc(currentWeekJobFireTimeUtc!.Value.DateTime, dutchTimeZone);
logger.LogInformation("CurrentWeekDiscountJob is scheduled to run next at: {CurrentWeekJobFireTime}", currentWeekJobFireTime);

// Stall indefinitely 
await Task.Delay(Timeout.Infinite);