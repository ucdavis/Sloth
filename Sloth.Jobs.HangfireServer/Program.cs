using System;
using System.Threading;
using Hangfire;
using Microsoft.Azure.WebJobs;

namespace Sloth.Jobs.HangfireServer
{
    public class Program
    {
        private static BackgroundJobServerOptions _options;
        private static CancellationToken _cancellationToken;

        public static void Main(string[] args)
        {
            try
            {
                // configure handfire job processor
                GlobalConfiguration.Configuration.UseSerilogLogProvider();

                // configure job server
                _options = new BackgroundJobServerOptions()
                {

                };

                // listen for shutdon
                _cancellationToken = new WebJobsShutdownWatcher().Token;

                // startup job server
                using (var server = new BackgroundJobServer(_options))
                {
                    while (true)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            server.SendStop();
                            break;
                        }

                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}