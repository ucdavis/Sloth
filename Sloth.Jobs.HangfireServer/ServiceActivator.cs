using System;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sloth.Jobs.HangfireServer.Logging;

namespace Sloth.Jobs.HangfireServer
{
    internal class ServiceActivator : JobActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override object ActivateJob(Type jobType)
        {
            return _serviceProvider.GetService(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ServiceActivatorScope(_serviceProvider, context);
        }
    }

    internal class ServiceActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope _scope;
        private readonly JobActivatorContext _context;

        public ServiceActivatorScope(IServiceProvider serviceProvider, JobActivatorContext context)
        {
            _scope = serviceProvider.CreateScope();
            _context = context;
        }

        public override object Resolve(Type type)
        {
            var provider = _scope.ServiceProvider;
            return provider.GetService(type);
        }

        public override void DisposeScope()
        {
            _scope?.Dispose();

            base.DisposeScope();
        }
    }
}
