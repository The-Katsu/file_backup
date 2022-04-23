using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

try
{
    var data = GetJsonData("config.json");
    CreateHostBuilder(args, data).Build().Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


static IHostBuilder CreateHostBuilder(string[] args, dynamic data) =>
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

                q.AddJob<FileBackupJob>(o => o
                    .WithIdentity(jobKey)
                    .UsingJobData("source", (string)data.source)
                    .UsingJobData("destination", (string)data.destination));

                q.AddTrigger(o => o
                    .ForJob(jobKey)
                    .WithIdentity("FileBackupJob-trigger")
                    .WithCronSchedule((string)data.frequency));
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        });

static dynamic GetJsonData(string path)
{
    using var sr = new StreamReader(path);
    var json = sr.ReadToEnd();
    dynamic data = Newtonsoft.Json.Linq.JObject.Parse(json);
    if (!Directory.Exists((string)data.source))
        throw new DirectoryNotFoundException($"Введённой директории-источника не существует - {data.source}");
    if (!Directory.Exists((string)data.destination))
        throw new DirectoryNotFoundException($"Введённой резервной директории не сущетсвует - {data.destination}");
    if (!CronExpression.IsValidExpression((string)data.frequency))
        throw new System.Data.InvalidExpressionException($"Неправильно введено cron-выражени - {data.frequency}" +
            "\n Генератор cron-выражений https://clck.ru/gMpAH");
    return data;
}