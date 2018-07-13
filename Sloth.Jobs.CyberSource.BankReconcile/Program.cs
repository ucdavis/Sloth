using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Core.Services;
using Sloth.Jobs.Core;

namespace Sloth.Jobs.CyberSource.BankReconcile
{
    public class Program : JobBase
    {
        public static void Main(string[] args)
        {
            var log = Log.ForContext("jobid", Guid.NewGuid());

            var assembyName = typeof(Program).Assembly.GetName();
            log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();

            // create job service
            var bankReconcileJob = provider.GetService<BankReconcileJob>();

            // call methods
            Task.Run(() => bankReconcileJob.UploadScrubber()).Wait();
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // options files
            services.Configure<CybersourceOptions>(Configuration.GetSection("Sloth"));

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<ISecretsService, SecretsService>();
            services.AddTransient<BankReconcileJob>();

            return services.BuildServiceProvider();
        }
    }
}
