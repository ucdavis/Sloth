﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using Serilog;
using Sloth.Api.Helpers;
using Sloth.Api.Jobs;
using Sloth.Core.Models;

namespace Sloth.Api.Services
{
    public interface IKfsScrubberService
    {
        Task<Uri> UploadScrubber(Scrubber scrubber, string filename, ILogger logger = null);
    }

    public class KfsScrubberService : IKfsScrubberService
    {
        private readonly IStorageService _storageService;

        private readonly string _username;
        private readonly string _password;
        private readonly string _host;

        private readonly string _storageContainer;

        public KfsScrubberService(IConfiguration configuration, IStorageService storageService)
        {
            _storageService = storageService;

            var kfsOptions = new KfsOptions();
            configuration.GetSection("Kfs").Bind(kfsOptions);

            _host = kfsOptions.Host;
            _username = kfsOptions.Username;
            _password = kfsOptions.Password;

            _storageContainer = kfsOptions.ScrubberBlobContainer;
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
            //using (var client = GetClient())
            //{
            //    client.Connect();

            //    ms.Seek(0, SeekOrigin.Begin);
            //    await Task.Factory.FromAsync(client.BeginUploadFile(ms, filename), client.EndUploadFile);
            //}

            return uri;
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
