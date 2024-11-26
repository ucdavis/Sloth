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
using Sloth.Core.Services;
using Sloth.Jobs.Core;

namespace Sloth.Jobs.Kfs.ScrubberUpload
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static async Task Main(string[] args)
        {

            return;
            //// setup env
            //Configure();

            //// TODO: create new record for new job type
            //// log run
            //var jobRecord = new JobRecord()
            //{
            //    Name       = KfsScrubberUploadJob.JobName,
            //    StartedAt  = DateTime.UtcNow,
            //    Status     = JobRecord.Statuses.Running,
            //};

            //_log = Log.Logger
            //    .ForContext("jobname", jobRecord.Name)
            //    .ForContext("jobid", jobRecord.Id);

            //var assembyName = typeof(Program).Assembly.GetName();
            //_log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            //// setup di
            //var provider = ConfigureServices();
            //var dbContext = provider.GetService<SlothDbContext>();

            //// save log to db
            //dbContext.JobRecords.Add(jobRecord);
            //dbContext.SaveChanges();

            //try
            //{
            //    // create job service
            //    var uploadScrubberJob = provider.GetService<KfsScrubberUploadJob>();

            //    // call methods
            //    var jobDetails = await uploadScrubberJob.UploadScrubber(_log);
            //    jobRecord.TotalTransactions = jobDetails.TransactionGroups.Select(g => g.TransactionCount).Sum();
            //    _log.Information("Finished");
            //    jobRecord.SetCompleted(JobRecord.Statuses.Finished, jobDetails);
            //}
            //catch (Exception ex)
            //{
            //    _log.Error("Unexpected error", ex);
            //    jobRecord.SetCompleted(JobRecord.Statuses.Failed, new());
            //}
            //finally
            //{
            //    await dbContext.SaveChangesAsync();
            //}
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<KfsScrubberOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<IKfsScrubberService, KfsScrubberService>();
            services.AddTransient<IStorageService, StorageService>();
            services.AddTransient<ISecretsService, SecretsService>();
            services.AddTransient<KfsScrubberUploadJob>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}
