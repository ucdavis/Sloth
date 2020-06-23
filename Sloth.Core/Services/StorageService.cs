using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;

namespace Sloth.Core.Services
{
    public interface IStorageService
    {
        Task GetBlobAsync(Stream stream, string containerName, string blobName);
        Task GetBlobAsync(Stream stream, Uri uri);
        Task<Uri> PutBlobAsync(Stream stream, string container, string filepath);
    }

    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public StorageService(IOptions<StorageServiceOptions> options)
        {
            // parse connection string
            var connectionString = options.Value.ConnectionString;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task GetBlobAsync(Stream stream, string containerName, string blobName)
        {
            var container = await GetBlobContainerAsync(containerName);
            var blob = container.GetBlockBlobClient(blobName);
            await blob.DownloadToAsync(stream);
        }

        public Task GetBlobAsync(Stream stream, Uri uri)
        {
            var blobUriBuilder = new BlobUriBuilder(uri);
            return GetBlobAsync(stream, blobUriBuilder.BlobContainerName, blobUriBuilder.BlobName);
        }

        public async Task<Uri> PutBlobAsync(Stream stream, string containerName, string blobName)
        {
            var container = await GetBlobContainerAsync(containerName);
            var blob = container.GetBlockBlobClient(blobName);
            await blob.UploadAsync(stream);

            return blob.Uri;
        }

        private async Task<BlobContainerClient> GetBlobContainerAsync(string containerName)
        {
            var client = _blobServiceClient.GetBlobContainerClient(containerName);
            await client.CreateIfNotExistsAsync();

            return client;
        }

    }
}
