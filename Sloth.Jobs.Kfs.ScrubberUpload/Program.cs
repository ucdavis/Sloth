using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sloth.Core;
using Sloth.Jobs.Core;
using Sloth.Jobs.Kfs.ScrubberUpload.Services;

namespace Sloth.Jobs.Kfs.ScrubberUpload
{
    public class Program : JobBase
    {
        static void Main(string[] args)
        {
            var log = Log.ForContext("jobid", Guid.NewGuid());

            var assembyName = typeof(Program).Assembly.GetName();
            log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

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

            // db service
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
            );

            // required services
            services.AddTransient<IKfsScrubberService, KfsScrubberService>();
            services.AddTransient<UploadScrubberJob>();

            return services.BuildServiceProvider();
        }
    }
}
