using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Jobs;
using Sloth.Core.Models;
using Sloth.Core.Services;
using Sloth.Web.Authorization;
using Sloth.Web.Handlers;
using Sloth.Web.Identity;
using Sloth.Web.Logging;
using Sloth.Web.Models;
using Sloth.Web.Resources;
using Sloth.Web.Services;
using StackifyLib;

namespace Sloth.Web
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

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add root configuration
            services.AddSingleton(Configuration);

            // add various options
            services.Configure<AppSettings>(Configuration);
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<CybersourceOptions>(Configuration.GetSection("Cybersource"));
            services.Configure<IamDirectorySearchServiceOptions>(Configuration.GetSection("IAM"));
            services.Configure<KfsScrubberOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));


            // add infrastructure services
            services.AddSingleton<IDirectorySearchService, IamDirectorySearchService>();
            services.AddSingleton<IKfsScrubberService, KfsScrubberService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddScoped<IWebHookService, WebHookService>();

            // add jobs services
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<CybersourceBankReconcileJob>();
            services.AddScoped<KfsScrubberUploadJob>();

            // add database connection
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SlothDbContext>()
                .AddUserManager<ApplicationUserManager>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                })
                .AddCAS("UCDavis", options =>
                {
                    options.CasServerUrlBase = Configuration["CasBaseUrl"];
                });

            services.AddAuthorization(o =>
            {
                o.AddPolicy(PolicyCodes.TeamAdmin,
                    policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Admin)));

                o.AddPolicy(PolicyCodes.TeamApprover,
                    policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Admin, TeamRole.Approver)));
            });
            services.AddScoped<IAuthorizationHandler, VerifyTeamPermissionHandler>();

            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            // setup logging
            LoggingConfiguration.Setup(Configuration);
            app.ConfigureStackifyLogging(Configuration);
            loggerFactory.AddSerilog();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<LoggingIdentityMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                // non team root routes
                routes.MapRoute(
                    name: "non-team-routes",
                    template: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "(account|jobs|system|users)" });

                routes.MapRoute(
                    name: "team-management-routes",
                    template: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "teams", action = "(index|create)" });

                // team level routes
                routes.MapRoute(
                    name: "team-index",
                    template: "{team}",
                    defaults: new { controller = "home", action = "teamindex" },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[] {
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                        })
                    });

                routes.MapRoute(
                    name: "team-routes",
                    template: "{team}/{controller=Home}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[] {
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                        })
                    });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
