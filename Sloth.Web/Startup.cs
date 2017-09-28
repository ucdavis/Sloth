using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
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
using Sloth.Core.Data;
using Sloth.Core.Models;
using Sloth.Core.Services;
using Sloth.Web.Identity;
using Sloth.Web.Logging;

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

            StackifyLib.Config.Environment = env.EnvironmentName;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add root configuration
            services.AddSingleton(Configuration);

            // add various options
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            
            // add logger configuration
            services.AddTransient(_ => LoggingConfiguration.Configuration);

            // add infrastructure services
            services.AddSingleton<IDirectorySearchService, DirectorySearchService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();

            // add database connection
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<User, Role>()
                .AddUserStore<UserStore>()
                .AddUserManager<UserManager<User>>()
                .AddRoleStore<RoleStore>()
                .AddRoleManager<RoleManager<Role>>();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, "CAS", options =>
                {
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime,
            SlothDbContext context)
        {
            // setup logging
            LoggingConfiguration.Setup(Configuration);

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
                    ReactHotModuleReplacement = true
                });

                DbInitializer.Initialize(context);
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

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
