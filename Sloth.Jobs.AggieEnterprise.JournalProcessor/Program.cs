using Sloth.Core.Models.Settings;
using Sloth.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Jobs;
using Sloth.Core.Models;
using Sloth.Jobs.Core;

namespace Sloth.Jobs.AggieEnterprise.JournalProcessor
{
    public class Program : JobBase
    {
        private static ILogger _log = null!;

        public static async Task Main(string[] args)
        {
            // setup env
            Configure();

            // TODO: create new record for new job type
            // log record to new table, add UX to show status and results

            // log run
            _log = Log.Logger
                .ForContext("jobname", AggieEnterpriseJournalJob.JobName);
            // .ForContext("jobid", jobRecord.Id);

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();
            // create services
            var dbContext = provider.GetRequiredService<SlothDbContext>();
            var journalJob = provider.GetRequiredService<AggieEnterpriseJournalJob>();

            // 1. check journal status for pending transactions
            var jobRecord = new JobRecord
            {
                Name = AggieEnterpriseJournalJob.JobNameUploadTransactions,
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running
            };
            dbContext.JobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();
            try
            {
                var aeJournalJobDetails = await journalJob.ResolveProcessingJournals(_log);
                jobRecord.TotalTransactions = aeJournalJobDetails.TransactionsProcessedCount;
                jobRecord.SetCompleted(JobRecord.Statuses.Finished, aeJournalJobDetails);
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

            // 2. upload all scheduled transactions
            jobRecord = new JobRecord
            {
                Name = AggieEnterpriseJournalJob.JobNameUploadTransactions,
                StartedAt = DateTime.UtcNow,
                Status = JobRecord.Statuses.Running
            };
            dbContext.JobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();
            try
            {
                var uploadTransactionsJobDetails = await journalJob.UploadTransactions(_log);
                jobRecord.TotalTransactions = uploadTransactionsJobDetails.TransactionsProcessedCount;
                jobRecord.SetCompleted(JobRecord.Statuses.Finished, uploadTransactionsJobDetails);
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
            services.Configure<AggieEnterpriseOptions>(Configuration.GetSection("AggieEnterprise"));
            services.Configure<SparkpostOptions>(Configuration.GetSection("SparkPost"));
            services.Configure<NotificationOptions>(Configuration.GetSection("Notifications"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<IAggieEnterpriseService, AggieEnterpriseService>();
            services.AddTransient<AggieEnterpriseJournalJob>();

            services.AddSingleton(_log);
            services.AddScoped<ISmtpService, SmtpService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services.BuildServiceProvider();
        }
    }
}
