using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Services;
using Sloth.Jobs.Core;
using Sloth.Jobs.Kfs.ScrubberUpload.Services;

namespace Sloth.Jobs.Kfs.ScrubberUpload
{
    public class Program : JobBase
    {
        private static ILogger _log;

        public static void Main(string[] args)
        {
            // setup env
            Configure();

            _log = Log.Logger
                .ForContext("jobname", "Kfs.ScrubberUpload")
                .ForContext("jobid", Guid.NewGuid());

            var assembyName = typeof(Program).Assembly.GetName();
            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();

            // create job service
            var uploadScrubberJob = provider.GetService<UploadScrubberJob>();

            // call methods
            Task.Run(() => uploadScrubberJob.UploadScrubber()).Wait();
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
            services.AddTransient<UploadScrubberJob>();

            services.AddSingleton(_log);

            return services.BuildServiceProvider();
        }
    }
}
