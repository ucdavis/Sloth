using System;
using System.IO;
using Hangfire;
using Hangfire.Console;
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
using Sloth.Api.Data;
using Sloth.Api.Identity;
using Sloth.Api.Logging;
using Sloth.Api.Swagger;
using Sloth.Core;
using Sloth.Core.Models;
using Sloth.Core.Services;
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
            // add root configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // add logger configuration
            services.AddTransient(_ => LoggingConfiguration.Configuration);
            
            // add infrastructure services
            services.AddSingleton<IKfsScrubberService, KfsScrubberService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();

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

            // add identity
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<SlothDbContext, Guid>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(o =>
            {
            });

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

            // add hangfire job activator
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
            // setup logging
            LoggingConfiguration.Setup(Configuration);

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

            // possibly reset db
            if (env.IsDevelopment())
            {
                DbInitializer.Initialize(context);
            }

            ConfigureHangfire(app, env);
        }

        private void ConfigureHangfire(IApplicationBuilder app, IHostingEnvironment env)
        {
            // setup hangfire storage
            GlobalConfiguration.Configuration
                .UseSerilogLogProvider()
                .UseConsole()
                .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
