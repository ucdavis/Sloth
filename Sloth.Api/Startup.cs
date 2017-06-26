using System.IO;
using Hangfire;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Sloth.Api.Identity;
using Sloth.Api.Jobs;
using Sloth.Api.Logging;
using Sloth.Api.Swagger;
using Sloth.Core;
using Swashbuckle.AspNetCore.Swagger;

namespace Sloth.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();

            // setup logging
            LoggingConfiguration.Setup(env, Configuration);

            // setup hangfire storage
            GlobalConfiguration.Configuration
                .UseSerilogLogProvider()
                .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"))
                .UseConsole();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add configuration options services
            services.Configure<StackifyOptions>(Configuration.GetSection("Stackify"));

            // add database connection
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // add framework services.
            services.AddMvc()
                .AddJsonOptions(o =>
                {
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            // add authentication policies
            services.AddAuthorization(o =>
            {
                o.AddPolicy("ApiKey", p => p.Requirements.Add(new ApiKeyRequirement()));
            });
            
            // add authentication handlers
            services.AddSingleton<IAuthorizationHandler, ApiKeyHandler>();

            // add swagger/swashbuckler
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Sloth API v1",
                    Version = "v1",
                    Description = "Scrubber Loader & Online Transaction Hub",
                    Contact = new Contact()
                    {
                        Name = "John Knoll",
                        Email = "jpknoll@ucdavis.edu"
                    },
                    License = new License()
                    {
                        Name = "MIT",
                        Url = "https://www.github.com/ucdavis/sloth/LICENSE"
                    },
                    Extensions =
                    {
                        { "ProjectUrl", "https://www.github.com/ucdavis/sloth" }
                    }
                });

                var xmlFilePath = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "Sloth.Api.xml");
                c.IncludeXmlComments(xmlFilePath);

                c.AddSecurityDefinition("apiKey", new ApiKeyScheme()
                {
                    Description = "API Key Authentication",
                    Name = "Authorization",
                    In = "header"
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            services.AddHangfire(c => { });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IApplicationLifetime appLifetime,
            SlothDbContext context)
        {
            loggerFactory.AddSerilog();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<LoggingIdentityMiddleware>();

            app.UseMvc();

            // add swagger ui
            app.UseSwagger(o =>
            {
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sloth API v1");
            });

            // add hangfire dashboard
            app.UseHangfireDashboard();
            //"/hangfire", new DashboardOptions()
            //{
            //    Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter(), }
            //});

            if (env.IsDevelopment())
            {
                //DbInitializer.Initialize(context);
            }

            // add recurring jobs
            GlobalConfiguration.Configuration
                .UseRecurringJob(typeof(Heartbeat));
        }
    }
}
