using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Models;
using Sloth.Core.Services;
using Sloth.Jobs.Filters;
using Sloth.Jobs.Jobs;
using Sloth.Jobs.Jobs.Attributes;
using Sloth.Jobs.Logging;
using Sloth.Jobs.Services;
using KfsOptions = Sloth.Jobs.Services.KfsOptions;

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
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add configurations
            services.AddSingleton(Configuration);
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<CybersourceOptions>(Configuration.GetSection("Cybersource"));
            services.Configure<KfsOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // add logger configuration
            services.AddTransient(_ => LoggingConfiguration.Configuration);

            // Add framework services.
            services.AddDbContext<SlothDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClientId = "c631afcb-0795-4546-844d-9fe7759ae620";
                    options.Authority = "https://login.microsoftonline.com/ucdavis365.onmicrosoft.com";
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.SetParameter("domain_hint", "ucdavis.edu");

                        return Task.FromResult(0);
                    };
                });

            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

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

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            ConfigureHangfire(app, env);
        }

        private void ConfigureHangfire(IApplicationBuilder app, IHostingEnvironment env)
        {
            // setup hangfire storage
            GlobalConfiguration.Configuration
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
