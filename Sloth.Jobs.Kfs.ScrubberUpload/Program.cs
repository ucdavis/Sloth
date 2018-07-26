using System;
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

        public static void Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var jobRecord = new KfsScrubberUploadJobRecord()
            {
                Id = Guid.NewGuid().ToString(),
                Name = KfsScrubberUploadJob.JobName,
                RanOn = DateTime.UtcNow,
                Status = "Running",
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
            dbContext.KfsScrubberUploadJobRecords.Add(jobRecord);
            dbContext.SaveChanges();

            try
            {
                // create job service
                var uploadScrubberJob = provider.GetService<KfsScrubberUploadJob>();

                // call methods
                Task.Run(() => uploadScrubberJob.UploadScrubber(_log)).Wait();
            }
            finally
            {
                // record status
                jobRecord.Status = "Finished";
                dbContext.SaveChanges();
            }
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
