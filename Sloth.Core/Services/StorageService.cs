using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        private readonly CloudStorageAccount _account;

        public StorageService(IConfiguration configuration)
        {
            // fetch options
            var options = new StorageServiceOptions();
            configuration.GetSection("Storage").Bind(options);

            // parse connection string
            var connectionString = options.ConnectionString;
            _account = CloudStorageAccount.Parse(connectionString);
        }

        public async Task GetBlobAsync(Stream stream, string containerName, string blobName)
        {
            var container = GetBlobContainer(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            await blob.DownloadToStreamAsync(stream);
        }

        public async Task GetBlobAsync(Stream stream, Uri uri)
        {
            var client = _account.CreateCloudBlobClient();
            var blob = client.GetBlobReferenceFromServer(uri);
            await blob.DownloadToStreamAsync(stream);
        }

        public async Task<Uri> PutBlobAsync(Stream stream, string containerName, string blobName)
        {
            var container = GetBlobContainer(containerName);
            var blob = container.GetBlockBlobReference(blobName);
            await blob.UploadFromStreamAsync(stream);

            return blob.Uri;
        }

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            var client = _account.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();

            return container;
        }

    }

    internal class StorageServiceOptions
    {
        public string ConnectionString { get; set; }
    }
}
