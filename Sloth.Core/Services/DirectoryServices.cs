using System.Threading.Tasks;
using AzureActiveDirectorySearcher;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;

namespace Sloth.Core.Services
{
    public interface IDirectorySearchService
    {
        Task<GraphUser> GetByKerb(string kerb);
    }

    public class DirectorySearchService : IDirectorySearchService
    {
        private readonly GraphSearchClient _client;

        public DirectorySearchService(IOptions<AzureOptions> configuration)
        {
            var azureOptions = configuration.Value;
            _client = new GraphSearchClient(new ActiveDirectoryConfigurationValues(azureOptions.TenantName,
                azureOptions.TentantId, azureOptions.ClientId, azureOptions.ClientSecret));
        }
        public Task<GraphUser> GetByKerb(string kerb)
        {
            return _client.GetUserByKerberos(kerb);
        }
    }

}
