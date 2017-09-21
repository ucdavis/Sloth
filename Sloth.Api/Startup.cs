using System;
using System.IO;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Sloth.Api.Data;
using Sloth.Api.Errors;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add root configuration
            services.AddSingleton<IConfiguration>(Configuration);

            // add logger configuration
            services.AddTransient(_ => LoggingConfiguration.Configuration);
            
            // add infrastructure services
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();

            // add database connection
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // cors support and policies
            services.AddCors(o => o.AddPolicy("AllowAnyOrgin",
                b => b.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod()
            ));

            // add framework services.
            services.AddMvc()
                .AddMvcOptions(o =>
                {
                    o.Filters.Add(new CorsAuthorizationFilterFactory("AllowAnyOrgin"));
                })
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
                
                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Sloth.Api.xml");
                c.IncludeXmlComments(xmlFilePath);

                c.AddSecurityDefinition("apiKey", new ApiKeyScheme()
                {
                    Description = "API Key Authentication",
                    Name = ApiKeyMiddleware.HeaderKey,
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

            if (env.IsDevelopment())
            {
                app.UseInternalExceptionMiddleware();
            }

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

            // setup hangfire storage
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
