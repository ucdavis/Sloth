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

namespace Sloth.Jobs.CyberSource.BankReconcile
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static async Task Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var jobRecord = new CybersourceBankReconcileJobRecord()
            {
                Name          = CybersourceBankReconcileJob.JobName,
                RanOn         = DateTime.UtcNow,
                Status        = "Running",
                ProcessedDate = yesterday,
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
            dbContext.CybersourceBankReconcileJobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();

            try
            {
                // create job service
                var bankReconcileJob = provider.GetService<CybersourceBankReconcileJob>();

                // call methods
                await foreach (var jobBlob in bankReconcileJob.ProcessReconcile(yesterday, jobRecord, _log))
                {
                    if (jobBlob == null) continue;
                    // save uploaded blob metadata
                    dbContext.CybersourceBankReconcileJobBlobs.Add(jobBlob);
                }
            }
            finally
            {
                // record status
                _log.Information("Finished");
                jobRecord.Status = "Finished";
                dbContext.SaveChanges();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<CybersourceOptions>(Configuration.GetSection("Cybersource"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<ISecretsService, SecretsService>();
            services.AddTransient<ICyberSourceBankReconcileService, CyberSourceBankReconcileService>();
            services.AddTransient<CybersourceBankReconcileJob>();
            services.AddTransient<IWebHookService, WebHookService>();
            services.AddTransient<IStorageService, StorageService>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}
