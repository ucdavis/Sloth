﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Jobs;
using Sloth.Core.Services;
using Sloth.Jobs.Core;

namespace Sloth.Jobs.AggieEnterprise.JournalProcessor
{
    public class Program : JobBase
    {
        private static ILogger _log = null!;

        public static void Main(string[] args)
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

            try
            {
                // create job service
                var journalJob = provider.GetService<AggieEnterpriseJournalJob>();

                // call methods

                // 1. upload all scheduled transactions
                journalJob?.UploadTransactions(_log).GetAwaiter().GetResult();

                // TODO: another method for checking journal statuses
            }
            finally
            {
                // record status
                _log.Information("Finished");
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<AggieEnterpriseOptions>(Configuration.GetSection("AggieEnterprise"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<IAggieEnterpriseService, AggieEnterpriseService>();
            services.AddTransient<AggieEnterpriseJournalJob>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}