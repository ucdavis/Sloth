using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Serilog;
using Sloth.Core.Extensions;
using Sloth.Core.Models;
using Sloth.Core.Services;

namespace Sloth.Jobs.Kfs.ScrubberUpload.Services
{
    public interface IKfsScrubberService
    {
        Task<Uri> UploadScrubber(Scrubber scrubber, string filename, ILogger logger = null);
    }

    public class KfsScrubberService : IKfsScrubberService
    {
        private readonly IStorageService _storageService;
        private readonly ISecretsService _secretsService;

        private readonly string _username;
        private readonly string _host;
        private readonly string _passwordKeyName;

        private PrivateKeyFile _privateKey;


        private readonly string _storageContainer;

        public KfsScrubberService(IOptions<KfsOptions> options, IStorageService storageService, ISecretsService secretsService)
        {
            _storageService = storageService;
            _secretsService = secretsService;

            _host = options.Value.Host;
            _username = options.Value.Username;

            _passwordKeyName = options.Value.PrivateFileName;

            _storageContainer = options.Value.ScrubberBlobContainer;
        }

        public async Task<Uri> UploadScrubber(Scrubber scrubber, string filename, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = Log.Logger;
            }

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            scrubber.ToXml(sw);

            sw.Flush();
            ms.Flush();

            // save copy of file online
            logger.ForContext("container", _storageContainer).Information("Uploading {filename} to Blog Storage", filename);
            ms.Seek(0, SeekOrigin.Begin);
            var uri = await _storageService.PutBlobAsync(ms, _storageContainer, filename);
            scrubber.Uri = uri.AbsoluteUri;

            // upload scrubber
            using (var client = await GetClient())
            {
                client.Connect();

                ms.Seek(0, SeekOrigin.Begin);
                await Task.Factory.FromAsync(client.BeginUploadFile(ms, filename), client.EndUploadFile);
            }

            return uri;
        }

        private async Task<SftpClient> GetClient()
        {
            var key = await GetPrivateKey();
            return new SftpClient(_host, 22, _username, key);
        }

        private async Task<PrivateKeyFile> GetPrivateKey()
        {
            if (_privateKey != null)
                return _privateKey;

            var encoded = await _secretsService.GetSecret(_passwordKeyName);
            var key = encoded.Base64Decode();
            _privateKey = new PrivateKeyFile(key.GenerateStreamFromString());
            return _privateKey;
        }
    }

    public class KfsOptions
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string PrivateFileName { get; set; }
        public string ScrubberBlobContainer { get; set; }
    }
}
