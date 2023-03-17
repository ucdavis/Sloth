using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Jobs;
using Sloth.Core.Models;
using Sloth.Core.Models.Settings;
using Sloth.Core.Services;
using Sloth.Jobs.Core;

namespace Sloth.Jobs.Notifications
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static async Task Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var jobRecord = new JobRecord()
            {
                Name = ResendPendingWebHookRequestsJob.JobName, //This is wrong...
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running,
            };

            _log = Log.Logger
                .ForContext("jobname", jobRecord.Name)
                .ForContext("jobid", jobRecord.Id);

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();
            var dbContext = provider.GetService<SlothDbContext>();

            // save log to db
            dbContext.JobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();
            try
            {
                // create job service
                var notificationJob = provider.GetService<NotificationJob>();

                var jobDetails = await notificationJob.ProcessNotifications();
                _log.Information("Finished");
                jobRecord.SetCompleted(JobRecord.Statuses.Finished, jobDetails);
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected error", ex);
                jobRecord.SetCompleted(JobRecord.Statuses.Failed, new());
            }
            finally
            {
                await dbContext.SaveChangesAsync();
            }

            jobRecord = new JobRecord()
            {
                Name = "Notify Reversals needing Approval", 
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running,
            };
            dbContext.JobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();

            try
            {
                // create job service
                var notificationJob = provider.GetService<NotificationJob>();

                var jobDetails = await notificationJob.ProcessNotifications(failures: false);
                _log.Information("Finished");
                jobRecord.SetCompleted(JobRecord.Statuses.Finished, jobDetails);
            }
            catch (Exception ex)
            {
                _log.Error("Unexpected error", ex);
                jobRecord.SetCompleted(JobRecord.Statuses.Failed, new());
            }
            finally
            {
                await dbContext.SaveChangesAsync();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<NotificationOptions>(Configuration.GetSection("Notifications"));
            services.Configure<SparkpostOptions>(Configuration.GetSection("Sparkpost"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<ISmtpService, SmtpService>();
            services.AddTransient<NotificationJob>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}
