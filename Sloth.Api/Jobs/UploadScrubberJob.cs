using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using Serilog;
using Sloth.Api.Helpers;
using Sloth.Api.Services;
using Sloth.Core;
using Sloth.Core.Models;

namespace Sloth.Api.Jobs
{
    public class UploadScrubberJob : JobBase
    {
        private readonly SlothDbContext _context;
        private readonly IStorageService _storageService;

        private readonly string _username;
        private readonly string _password;
        private readonly string _host;
        private readonly string _storageContainer;

        public UploadScrubberJob(SlothDbContext context, IKfsScrubberService kfsScrubberService)
        {
            _context = context;
            _storageService = storageService;

            var kfsOptions = new KfsOptions();
            configuration.GetSection("Kfs").Bind(kfsOptions);

            _host = kfsOptions.Host;
            _username = kfsOptions.Username;
            _password = kfsOptions.Password;

            _storageContainer = kfsOptions.ScrubberBlobContainer;
        }

        [RecurringJob(CronStrings.EndOfBusiness, RecurringJobId = "upload-nightly-scrubber")]
        public async Task UploadScrubber(PerformContext context)
        {
            SetupLogging(context);

            var log = Logger.ForContext("jobId", context.BackgroundJob.Id);

            try
            {
                // fetch all staged transactions
                var transactions = _context.Transactions
                    .Where(t => t.Status == TransactionStatus.Scheduled)
                    .ToList();

                // create scrubber
                log.Information("Creating Scrubber for {count} transactions.", transactions.Count);
                var scrubber = new Scrubber()
                {
                    Chart               = "3",
                    OrganizationCode    = "ACCT",
                    BatchDate           = DateTime.Today,
                    BatchSequenceNumber = 1,
                    Transactions        = transactions
                };
                
                // create filename
                var oc = "SL";
                var filename = $"journal.{oc}.{DateTime.UtcNow:yyyyMMddHHmmssffff}.xml";

                // serialize scrubber
                log.Information("Serializing {filename}", filename);
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms);
                scrubber.ToXml(sw);

                sw.Flush();
                ms.Flush();

                // save copy of file online
                log.ForContext("container", _storageContainer).Information("Uploading file to Blog Storage");
                ms.Seek(0, SeekOrigin.Begin);
                var uri = await _storageService.PutBlobAsync(ms, _storageContainer, filename);
                scrubber.Uri = uri.AbsoluteUri;

                // upload scrubber
                //using (var client = GetClient())
                //{
                //    client.Connect();

                //    ms.Seek(0, SeekOrigin.Begin);
                //    await Task.Factory.FromAsync(client.BeginUploadFile(ms, filename), client.EndUploadFile);
                //}

                // persist scrubber
                _context.Scrubbers.Add(scrubber);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }

        private SftpClient GetClient()
        {
            return new SftpClient(_host, 22, _username, _password);
        }
    }

    internal class KfsOptions
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ScrubberBlobContainer { get; set; }
    }
}
