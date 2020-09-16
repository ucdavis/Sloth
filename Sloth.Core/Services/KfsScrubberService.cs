using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Serilog;
using Sloth.Core.Extensions;
using Sloth.Core.Models;

namespace Sloth.Core.Services
{
    public interface IKfsScrubberService
    {
        Task<Uri> UploadScrubber(Scrubber scrubber, string filename, string username, string passwordKeyName, ILogger logger = null);
        Task VerifyConnection(string username, string passwordKeyName);
    }

    public class KfsScrubberService : IKfsScrubberService
    {
        private readonly IStorageService _storageService;
        private readonly ISecretsService _secretsService;

        private readonly string _host;
        private readonly string _storageContainer;

        public KfsScrubberService(IOptions<KfsScrubberOptions> options, IStorageService storageService, ISecretsService secretsService)
        {
            _storageService = storageService;
            _secretsService = secretsService;

            _host = options.Value.Host;
            _storageContainer = options.Value.ScrubberBlobContainer;
        }

        public async Task<Uri> UploadScrubber(Scrubber scrubber, string filename, string username, string passwordKeyName, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = Log.Logger;
            }

            await using var ms = new MemoryStream();
            await using var sw = new StreamWriter(ms);
            scrubber.ToXml(sw);

            await sw.FlushAsync();
            await ms.FlushAsync();

            Uri uri = default;

            try
            {
                // save copy of file online
                logger.ForContext("container", _storageContainer)
                    .Information("Uploading {filename} to Blob Storage", filename);
                ms.Seek(0, SeekOrigin.Begin);
                uri = await _storageService.PutBlobAsync(ms, _storageContainer, filename);
                scrubber.Uri = uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                logger.ForContext("container", _storageContainer)
                    .Error(ex, ex.Message);
            }

            // upload scrubber
            using var client = await GetClient(username, passwordKeyName);
            client.Connect();

            ms.Seek(0, SeekOrigin.Begin);
            await Task.Factory.FromAsync(client.BeginUploadFile(ms, filename), client.EndUploadFile);

            return uri;
        }

        public async Task VerifyConnection(string username, string passwordKeyName) {
            using (var client = await GetClient(username, passwordKeyName))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        private async Task<SftpClient> GetClient(string username, string passwordKeyName)
        {
            var key = await GetPrivateKey(passwordKeyName);
            return new SftpClient(_host, 22, username, key);
        }

        private async Task<PrivateKeyFile> GetPrivateKey(string passwordKeyName)
        {
            var encoded = await _secretsService.GetSecret(passwordKeyName);
            var key = encoded.Base64Decode();
            return new PrivateKeyFile(key.GenerateStreamFromString());
        }
    }

    public class KfsScrubberOptions
    {
        public string Host { get; set; }
        public string ScrubberBlobContainer { get; set; }
    }
}
