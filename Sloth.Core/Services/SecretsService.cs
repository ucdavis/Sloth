using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        private readonly SecretServiceSettings _settings;

        public SecretsService(IConfiguration configuration)
        {
            _settings = new SecretServiceSettings();
            configuration.GetSection("KeyVaultService").Bind(_settings);

            _vault = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var credential = new ClientCredential(_settings.ClientId, _settings.ClientSecret);
                var token = await authContext.AcquireTokenAsync(resource, credential);

                return token.AccessToken;

            });
        }

        public async Task<string> GetSecret(string name)
        {
            var result = await _vault.GetSecretAsync(_settings.Url, name);
            return result.Value;
        }

        public async Task UpdateSecret(string name, string value)
        {
            await _vault.SetSecretAsync(_settings.Url, name, value);
        }
    }

    internal class SecretServiceSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Url { get; set; }
    }
}
