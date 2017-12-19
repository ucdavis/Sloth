using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sloth.Core.Configuration;
using Sloth.Core.Extensions;

namespace Sloth.Core.Services
{
    public interface IKfsService
    {
        Task<Account> GetAccount(string chart, string account);
        Task<bool> IsAccountValid(string chart, string account);
    }

    public class KfsService : IKfsService
    {
        private readonly Uri _baseUrl;

        public KfsService(IOptions<KfsOptions> options)
        {
            var baseUrl = options.Value.ApiBaseUrl;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _baseUrl))
            {
                throw new ArgumentException("BaseUrl must be valid Absolute URI", nameof(options.Value.ApiBaseUrl));
            }
        }

        public async Task<Account> GetAccount(string chart, string account)
        {
            chart = Uri.EscapeDataString(chart);
            account = Uri.EscapeDataString(account);

            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync($"fau/account/{chart}/{account}");
                return await response.GetContentOrNullAsync<Account>();
            }
        }

        public async Task<bool> IsAccountValid(string chart, string account)
        {
            chart = Uri.EscapeDataString(chart);
            account = Uri.EscapeDataString(account);

            using (var client = GetHttpClient())
            {
                var response = await client.GetAsync($"fau/account/{chart}/{account}/isvalid");
                return await response.GetContentOrNullAsync<bool>();
            }
        }



        private HttpClient GetHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = _baseUrl
            };

            return client;
        }
    }

    public class Account
    {
        [JsonProperty("chartOfAccountsCode")]
        public string Chart { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("organizationCode")]
        public string OrganizationCode { get; set; }

        [JsonProperty("organizationName")]
        public string OragnizationName { get; set; }

        [JsonProperty("accountTypeCode")]
        public string AccountTypeCode { get; set; }

        [JsonProperty("accountTypeName")]
        public string AccountTypeName { get; set; }

        [JsonProperty("fiscalOfficer")]
        public Person FiscalOfficer { get; set; }

        [JsonProperty("accountManager")]
        public Person AccountManager { get; set; }

        [JsonProperty("accountSupervisor")]
        public Person AccountSupervisor { get; set; }

        [JsonProperty("ucLocationCode")]
        public string UcLocationCode { get; set; }

        [JsonProperty("sauCode")]
        public string SauCode { get; set; }

        [JsonProperty("ucAccountName")]
        public string UcAccountName { get; set; }

        [JsonProperty("ucFundNumber")]
        public string UcFundNumber { get; set; }

        [JsonProperty("ucFundName")]
        public string UcFundName { get; set; }

        [JsonProperty("subFundGroupCode")]
        public string SubFundGroupCode { get; set; }

        [JsonProperty("subFundGroupName")]
        public string SubFundGroupName { get; set; }

        [JsonProperty("subFundGroupTypeCode")]
        public string SubFundGroupTypeCode { get; set; }

        [JsonProperty("ucFundGroup")]
        public string UcFundGroup { get; set; }
    }

    public class Person
    {
        [JsonProperty("principalId")]
        public string PrincipalId { get; set; }

        [JsonProperty("principalName")]
        public string PrincipalName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }
    }
}
