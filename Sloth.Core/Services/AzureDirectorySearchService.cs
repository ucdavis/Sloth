using System.Threading.Tasks;
using AzureActiveDirectorySearcher;
using Microsoft.Extensions.Options;
using Sloth.Core.Configuration;

namespace Sloth.Core.Services
{
    public class AzureDirectorySearchService : IDirectorySearchService
    {
        private readonly GraphSearchClient _client;

        public AzureDirectorySearchService(IOptions<AzureOptions> configuration)
        {
            var azureOptions = configuration.Value;
            _client = new GraphSearchClient(new ActiveDirectoryConfigurationValues(
                azureOptions.TenantName,
                azureOptions.TentantId,
                azureOptions.ClientId,
                azureOptions.ClientSecret));
        }
        public async Task<DirectoryUser> GetByKerberos(string kerb)
        {
            var result = await _client.GetUserByKerberos(kerb);
            return TransformGraphResult(result);
        }

        public async Task<DirectoryUser> GetByEmail(string email)
        {
            var result = await _client.GetUserByEmail(email);
            return TransformGraphResult(result);
        }

        private static DirectoryUser TransformGraphResult(GraphUser result)
        {
            return new DirectoryUser()
            {
                Email     = result.Mail,
                GivenName = result.GivenName,
                Surname   = result.Surname,
                Kerberos  = result.Kerberos,
            };
        }
    }
}
