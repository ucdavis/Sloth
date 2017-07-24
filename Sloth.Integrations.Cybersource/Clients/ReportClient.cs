using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Sloth.Integrations.Cybersource.Exceptions;
using Sloth.Integrations.Cybersource.Helpers;

namespace Sloth.Integrations.Cybersource.Clients
{
    public class ReportClient
    {
        private readonly string _baseUri;
        private readonly string _merchantId;
        private readonly string _username;
        private readonly string _password;


        public ReportClient(string baseUri, string merchantId, string username, string password)
        {
            _baseUri = baseUri;
            _merchantId = merchantId;
            _username = username;
            _password = password;
        }


        public async Task<Report> GetPaymentBatchDetailReport(DateTime date)
        {
            // fetch content
            var content = await GetClientApiReport("PaymentBatchDetailReport", date);

            // deserialize
            return DeserializeReport<Report>(content);
        }

        private async Task<string> GetClientApiReport(string reportName, DateTime date)
        {
            // build uri
            var uri = $"{_baseUri}/DownloadReport/{date:yyyy}/{date:MM}/{date:dd}/{_merchantId}/{reportName}.xml";
            var client = GetHttpClient();
            var response = await client.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ReportNotFoundException();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            response.EnsureSuccessStatusCode();

            // fetch content
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get configured http cliewnt
        /// </summary>
        /// <returns></returns>
        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();

            //specify to use TLS 1.2 as default connection
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            client.DefaultRequestHeaders.Authorization = GetCredentials();
            client.Timeout = new TimeSpan(0, 0, 0, 10);

            return client;
        }

        /// <summary>
        /// Get Credential Headers
        /// </summary>
        /// <returns></returns>
        private AuthenticationHeaderValue GetCredentials()
        {
            // basic authentication
            var username = _username;
            var password = _password;
            var credentials = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(
                    String.Format("{0}:{1}", username, password))));

            return credentials;
        }

        /// <summary>
        /// Deserialize XML
        /// </summary>
        /// <typeparam name="TReport"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static TReport DeserializeReport<TReport>(string xml) where TReport : class
        {
            // strip namespaces
            xml = XmlHelpers.RemoveAllNamespaces(xml);

            // load xml
            var stream = new StringReader(xml);

            // deserialize
            var serializer = new XmlSerializer(typeof(TReport));
            return serializer.Deserialize(stream) as TReport;
        }
    }
}
