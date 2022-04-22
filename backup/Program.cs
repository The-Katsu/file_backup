using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Trace);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                var jobKey = new JobKey("FileBackupJob");

                q.AddJob<FileBackUpJob>(o => o.WithIdentity(jobKey));

                q.AddTrigger(o => o
                    .ForJob(jobKey)
                    .WithIdentity("FileBackupJob-trigger")
                    .WithCronSchedule("* * * * * ? *"));
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        });