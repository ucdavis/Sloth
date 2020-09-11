using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Sloth.Api.Errors;
using Sloth.Api.Identity;
using Sloth.Api.Logging;
using Sloth.Api.Models;
using Sloth.Api.Swagger;
using Sloth.Core;
using Sloth.Core.Configuration;
using Sloth.Core.Services;
using StackifyLib;

namespace Sloth.Api
{
    public class Startup
    {
        private readonly string CorsPolicyAllowAnyOrgin = "CorsPolicyAllowAnyOrgin";

        public Startup(IWebHostEnvironment env)
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
            // add configuration
            services.AddSingleton(Configuration);
            services.Configure<AppSettings>(Configuration);
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<KfsOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // add infrastructure services
            services.AddSingleton<IKfsService, KfsService>();
            services.AddSingleton<ISecretsService, SecretsService>();
            services.AddSingleton<IStorageService, StorageService>();

            // add database connection
            services.AddDbContext<SlothDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // cors support and policies
            services.AddCors(o => o.AddPolicy(CorsPolicyAllowAnyOrgin,
                b => b.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod()
            ));

            // add framework services.
            services.AddMvc()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.Formatting = Formatting.Indented;
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
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
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Sloth API v1",
                    Version = "v1",
                    Description = "Scrubber Loader & Online Transaction Hub",
                    Contact = new OpenApiContact
                    {
                        Name = "Application Support",
                        Email = "apprequests@caes.ucdavis.edu"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://github.com/ucdavis/Sloth/blob/master/LICENSE")
                    },
                    Extensions =
                    {
                        { "ProjectUrl", new OpenApiString("https://github.com/ucdavis/Sloth/") }
                    }
                });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Sloth.Api.xml");
                c.IncludeXmlComments(xmlFilePath);

                var securityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    Type = SecuritySchemeType.ApiKey,
                    Description = "API Key Authentication",
                    Name = ApiKeyMiddleware.HeaderKey,
                    In = ParameterLocation.Header,
                    Scheme = "ApiKey"
                };

                c.AddSecurityDefinition("ApiKey", securityScheme);

                c.OperationFilter<SecurityRequirementsOperationFilter>(securityScheme);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime appLifetime)
        {
            // setup logging
            LoggingConfiguration.Setup(Configuration);
            app.ConfigureStackifyLogging(Configuration);
            loggerFactory.AddSerilog();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<LoggingIdentityMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseInternalExceptionMiddleware();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseCors(CorsPolicyAllowAnyOrgin);

            // add swagger ui
            app.UseSwagger(o =>
            {
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sloth API v1");
            });
        }
    }
}
