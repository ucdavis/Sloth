using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;
using Sloth.Core.Models;

namespace Sloth.Core.Services
{
    public interface IStorageService
    {
        Task<Stream> GetBlobAsync(string containerName, string blobName);
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

        public async Task<Stream> GetBlobAsync(string containerName, string blobName)
        {
            var stream = new MemoryStream();
            var containerClient = await GetBlobContainerAsync(containerName);
            var blobClient = containerClient.GetBlockBlobClient(blobName);
            await blobClient.DownloadToAsync(stream);
            return stream;
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

    public static class StorageServiceExtensions
    {
        public static Task<Stream> GetBlobAsync(this IStorageService storageService, Uri uri)
        {
            var blobUriBuilder = new BlobUriBuilder(uri);
            return storageService.GetBlobAsync(blobUriBuilder.BlobContainerName, blobUriBuilder.BlobName);
        }

        public static Task<Stream> GetBlobAsync(this IStorageService storageService, string uri)
        {
            return storageService.GetBlobAsync(new Uri(uri));
        }

        public static Task<Stream> GetBlobAsync(this IStorageService storageService, Blob blob)
        {
            return storageService.GetBlobAsync(new Uri(blob.Uri));
        }

        public static async Task<Blob> PutBlobAsync(this IStorageService storageService, Stream stream, Blob blob)
        {
            var uri = await storageService.PutBlobAsync(stream, blob.Container, blob.FileName);
            blob.Uri = uri.AbsoluteUri;
            blob.UploadedDate = DateTime.UtcNow;
            return blob;
        }

        public static Task<Blob> PutBlobAsync(this IStorageService storageService, string localFilePath, Blob blob)
        {
            var stream = File.OpenRead(localFilePath);
            return storageService.PutBlobAsync(stream, blob);
        }

        public static async Task<Blob> PutBlobAsync(this IStorageService storageService, Stream stream,
            string container, string name, string desctiption, string mediaType)
        {
            var blob = new Blob
            {
                FileName = name,
                Description = desctiption,
                Container = container,
                MediaType = mediaType,
            };
            await storageService.PutBlobAsync(stream, blob);
            return blob;
        }

        public static Task<Blob> PutBlobAsync(this IStorageService storageService, string localFilePath,
            string container, string name, string desctiption, string mediaType)
        {
            var stream = File.OpenRead(localFilePath);
            return storageService.PutBlobAsync(stream, container, name, desctiption, mediaType);
        }
    }
}
