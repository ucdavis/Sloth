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

namespace Sloth.Jobs.WebHooks.Resend
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static async Task Main(string[] args)
        {
            // setup env
            Configure();

            // log run
            var jobRecord = new WebHookRequestResendJobRecord()
            {
                Name          = WebHookRequestResendJobRecord.JobName,
                RanOn         = DateTime.UtcNow,
                Status        = "Running",
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
            dbContext.WebHookRequestResendJobRecords.Add(jobRecord);
            await dbContext.SaveChangesAsync();

            try
            {
                // create job service
                var resendWebHookJob = provider.GetService<ResendPendingWebHookRequestsJob>();

                // call methods
                foreach (var webHookRequest in await resendWebHookJob.ResendPendingWebHookRequests())
                {
                    webHookRequest.WebHookRequestResendJobId = jobRecord.Id;
                }
            }
            finally
            {
                // record status
                _log.Information("Finished");
                jobRecord.Status = "Finished";
                await dbContext.SaveChangesAsync();
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<WebHookServiceOptions>(Configuration.GetSection("WebHooks"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<ISecretsService, SecretsService>();
            services.AddTransient<ResendPendingWebHookRequestsJob>();
            services.AddTransient<IWebHookService, WebHookService>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}
