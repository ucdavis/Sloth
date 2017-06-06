﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sloth.Core;

namespace Sloth.Api.Identity
{
    public class ApiKeyMiddleware
    {
        private const string HeaderKey = "X-Auth-Token";

        private readonly RequestDelegate _next;
        private readonly SlothDbContext _dbContext;
        private readonly ApiKeyProviderOptions _options;
        private ILogger _logger;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyProviderOptions> options, ILoggerFactory loggerFactory, SlothDbContext dbContext)
        {
            _next = next;
            _dbContext = dbContext;
            _options = options.Value;
            _logger = loggerFactory.CreateLogger<ApiKeyMiddleware>();
        }

        public Task Invoke(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(HeaderKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsync($"Missing {HeaderKey} Header");
            }

            var headerValue = context.Request.Headers[HeaderKey].FirstOrDefault();

            // lookup apikey from db
            var apiKey = _dbContext.ApiKeys.Find(headerValue);
            if (apiKey == null || apiKey.Revoked.HasValue)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return context.Response.WriteAsync($"Invalid {HeaderKey} Header");
            }
            
            context.User.AddIdentity(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, apiKey.Owner)
            }));

            return _next(context);
        }
    }

    public class ApiKeyProviderOptions
    {
    }
}