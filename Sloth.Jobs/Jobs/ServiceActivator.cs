using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Sloth.Jobs.Jobs
{
    public class ServiceActivator : JobActivator
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
