using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using SpaCliMiddleware;
using StackifyLib;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Sloth.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            services.Configure<WebHookServiceOptions>(Configuration.GetSection("WebHooks"));


            // add infrastructure services
            services.AddSingleton<IDirectorySearchService, IamDirectorySearchService>();
            services.AddSingleton<IKfsScrubberService, KfsScrubberService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddScoped<IWebHookService, WebHookService>();

            // add jobs services
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<ICyberSourceBankReconcileService, CyberSourceBankReconcileService>();
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
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

#if DEBUG
            // Enable recompiling views without restarting app
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ConfigureStackifyLogging(Configuration);

            app.UseSerilogRequestLogging();
            app.UseMiddleware<CorrelationIdMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseMiddleware<LoggingIdentityMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(routes =>
            {
                // non team root routes
                routes.MapControllerRoute(
                    name: "non-team-routes",
                    pattern: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "(account|jobs|system|users)" });

                routes.MapControllerRoute(
                    name: "team-management-routes",
                    pattern: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "teams", action = "(index|create)" });

                // team level routes
                routes.MapControllerRoute(
                    name: "team-index",
                    pattern: "{team}",
                    defaults: new { controller = "home", action = "teamindex" },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[] {
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                        })
                    });

                routes.MapControllerRoute(
                    name: "team-routes",
                    pattern: "{team}/{controller=Home}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[] {
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                        })
                    });

                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                if (env.IsDevelopment())
                {
                    routes.MapToSpaCliProxy(
                        "{*path}",
                        options: new SpaOptions { SourcePath = "wwwroot/dist" },
                        npmScript: "devpack",
                        port: /*default(int)*/ 8080, // Allow webpack to find own port
                        regex: "Project is running",
                        forceKill: true, // kill anything running on our webpack port
                        useProxy: true, // proxy webpack requests back through our aspnet server
                        runner: ScriptRunnerType.Npm
                    );
                }

            });
        }
    }
}
