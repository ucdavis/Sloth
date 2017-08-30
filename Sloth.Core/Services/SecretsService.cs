using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Sloth.Core.Services
{
    public interface ISecretsService
    {
        Task<string> GetSecret(string name);
        Task UpdateSecret(string name, string value);
    }

    public class SecretsService : ISecretsService
    {
        private readonly KeyVaultClient _vault;
        private readonly SecretServiceOptions _options;

        public SecretsService(IOptions<SecretServiceOptions> options)
        {
            _options = options.Value;

            _vault = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var credential = new ClientCredential(_options.ClientId, _options.ClientSecret);
                var token = await authContext.AcquireTokenAsync(resource, credential);

                return token.AccessToken;

            });
        }

        public async Task<string> GetSecret(string name)
        {
            var result = await _vault.GetSecretAsync(_options.Url, name);
            return result.Value;
        }

        public async Task UpdateSecret(string name, string value)
        {
            await _vault.SetSecretAsync(_options.Url, name, value);
        }
    }

    public class SecretServiceOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
    }
}
