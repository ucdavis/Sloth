using System;
using System.Security.Claims;
using AspNetCore.Security.CAS;
using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Models;
using Sloth.Core.Services;
using Sloth.Jobs.Filters;
using Sloth.Jobs.Jobs;
using Sloth.Jobs.Jobs.Attributes;
using Sloth.Jobs.Logging;
using Sloth.Jobs.Services;

namespace Sloth.Jobs
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add configurations
            services.AddSingleton<IConfiguration>(Configuration);
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<CybersourceOptions>(Configuration.GetSection("Cybersource"));
            services.Configure<KfsOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // add logger configuration
            services.AddTransient(_ => LoggingConfiguration.Configuration);

            // Add framework services.
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SlothDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(o => { });

            services.AddMvc();

            services.AddHangfire(c => { });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // add infrastructure services
            services.AddTransient<IDirectorySearchService, DirectorySearchService>();
            services.AddTransient<IKfsScrubberService, KfsScrubberService>();
            services.AddTransient<ISecretsService, SecretsService>();
            services.AddTransient<IStorageService, StorageService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            ConfigureAuthentication(app);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ConfigureHangfire(app, env);
        }

        private void ConfigureAuthentication(IApplicationBuilder app)
        {
            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
            var casOptions = new CasOptions
            {
                CasServerUrlBase = "https://cas.ucdavis.edu/cas/",
                Events = new CasEvents
                {
                    OnCreatingTicket = async ctx =>
                    {
                        var identity = ctx.Principal.Identity as ClaimsIdentity;
                        if (identity == null)
                        {
                            return;
                        }

                        var kerb = identity.FindFirst(ClaimTypes.NameIdentifier).Value;

                        // look up user info and add as claims
                        var user = await app.ApplicationServices.GetService<IDirectorySearchService>().GetByKerb(kerb);

                        if (user != null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, user.Mail));
                            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.GivenName));
                            identity.AddClaim(new Claim(ClaimTypes.Surname, user.Surname));

                            // Cas already adds a name param but it's a duplicate of nameIdentifier, so let's replace with something useful
                            identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.DisplayName));
                        }
                    }
                }
            };
            app.UseCasAuthentication(casOptions);
        }

        private void ConfigureHangfire(IApplicationBuilder app, IHostingEnvironment env)
        {
            // setup hangfire storage
            GlobalConfiguration.Configuration
                .UseSerilogLogProvider()
                .UseConsole()
                .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));

            // add recurring jobs
            GlobalConfiguration.Configuration
                .UseRecurringJob(typeof(Heartbeat))
                .UseRecurringJob(typeof(CybersourceBankDepositJob))
                .UseRecurringJob(typeof(UploadScrubberJob));

            // setup action filters
            GlobalJobFilters.Filters.Add(new JobContextLoggerAttribute());

            app.UseHangfireDashboard(
            "/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new AdminAuthorizationFilter(), }
            });

            if (env.IsDevelopment())
            {
                app.UseHangfireServer();
            }
        }
    }
}
