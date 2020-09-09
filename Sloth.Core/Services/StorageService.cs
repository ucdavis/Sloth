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
            var containerClient = await GetBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            await blobClient.DownloadToAsync(stream);
        }

        public Task GetBlobAsync(Stream stream, Uri uri)
        {
            var blobUriBuilder = new BlobUriBuilder(uri);
            return GetBlobAsync(stream, blobUriBuilder.BlobContainerName, blobUriBuilder.BlobName);
        }

        public async Task<Uri> PutBlobAsync(Stream stream, string containerName, string blobName)
        {
            var containerClient = await GetBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            await blobClient.UploadAsync(stream);

            return blobClient.Uri;
        }

        private async Task<BlobContainerClient> GetBlobContainerAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            return containerClient;
        }

    }
}
