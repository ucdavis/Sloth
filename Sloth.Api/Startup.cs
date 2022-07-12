using System;
using System.IO;
using System.Linq;
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

namespace Sloth.Api
{
    public class Startup
    {
        private readonly string CorsPolicyAllowAnyOrgin = "CorsPolicyAllowAnyOrgin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add configuration
            services.AddSingleton(Configuration);
            services.Configure<AppSettings>(Configuration);
            services.Configure<AggieEnterpriseOptions>(Configuration.GetSection("AggieEnterprise"));
            services.Configure<AzureOptions>(Configuration.GetSection("Azure"));
            services.Configure<KfsOptions>(Configuration.GetSection("Kfs"));
            services.Configure<StorageServiceOptions>(Configuration.GetSection("Storage"));

            // add infrastructure services
            services.AddSingleton<IKfsService, KfsService>();
            services.AddSingleton<IAggieEnterpriseService, AggieEnterpriseService>();
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

                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "Sloth API v2 (Aggie Enterprise)",
                    Version = "v2",
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

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);

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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSerilogRequestLogging();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ApiKeyMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseInternalExceptionMiddleware();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<LoggingIdentityMiddleware>();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseCors(CorsPolicyAllowAnyOrgin);

            // add swagger ui
            app.UseSwagger(o =>
            {
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sloth API v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Sloth API V2");
            });
        }
    }
}
