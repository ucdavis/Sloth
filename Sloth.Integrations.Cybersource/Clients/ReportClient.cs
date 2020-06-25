using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RestSharp;
using Sloth.Integrations.CyberSource;
using Sloth.Integrations.Cybersource.Exceptions;
using Sloth.Integrations.Cybersource.Helpers;

namespace Sloth.Integrations.Cybersource.Clients
{
    public class ReportClient
    {
        private readonly bool _isProduction;
        private readonly string _merchantId;
        private readonly string _keyId;
        private readonly string _secret;

        public ReportClient(bool isProduction, string merchantId, string keyId, string secret)
        {
            _isProduction = isProduction;
            _merchantId = merchantId;
            _keyId = keyId;
            _secret = secret;
        }


        public Task<Report> GetPaymentBatchDetailReport(DateTime date)
        {
            /*
             * This report requires more information than what is made available by the standard report.
             * Specifically, a custom report should be created on the cybersource portal with the following:
             * Name: PaymentBatchDetailReport_Full
             * Format: XML
             * Frequency: Daily
             * Start Time: 12AM
             * TimeZone: PST
             * Custom Fields:
             *      Application
             *      Batch
             *      BillTo
             *      PaymentData
             *          Amount
             *          PaymentRequestId
             *          TransactionRefNumber
             *
             * Other fields are okay, but not necessary
             */

            // fetch content
            return GetOneTimePaymentBatchDetailReport("PaymentBatchDetailReport_Full", date);
        }

        public async Task<Report> GetOneTimePaymentBatchDetailReport(string reportName, DateTime date)
        {
            // fetch content
            var content = await GetClientApiReport<Report>(reportName, date);

            return DeserializeReport<Report>(content);
        }

        private async Task<string> GetClientApiReport<T>(string reportName, DateTime date)
        {
            // build uri
            var uri = "reporting/v3/report-downloads";
            var client = GetApiClient();

            // build request
            var request = new RestRequest(uri, Method.GET);
            request.AddQueryParameter("organizationId", _merchantId);
            request.AddQueryParameter("reportDate", date.ToString("yyyy-MM-dd"));
            request.AddQueryParameter("reportName", reportName);

            request.AddHeader("Accept", "*/*");
            request.AddHeader("Content-Type", "application/xml");

            // add auth
            AddApiKeyAuthenticationHeaders(client, request);

            // execute
            var response = await client.ExecuteGetAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ReportNotFoundException();
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            // fetch content
            return response.Content;
        }

        /// <summary>
        /// Get configured http client
        /// </summary>
        /// <returns></returns>
        private IRestClient GetApiClient()
        {
            var baseUrl = _isProduction
                ? "https://api.cybersource.com"
                : "https://apitest.cybersource.com";

            var client = new RestClient(baseUrl);
            return client;
        }

        private void AddApiKeyAuthenticationHeaders(IRestClient client, IRestRequest request)
        {
            // parse out request values
            var method = request.Method;
            var uri = client.BuildUri(request);
            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);

            // configure signer
            var signer = new SignatureRequest()
            {
                MerchantId = _merchantId,
                KeyId      = _keyId,
                KeySecret  = _secret,
                HostName   = uri.Host,
                Method     = request.Method,
                TargetUri  = uri,
                JsonBody   = body?.ToString()
            };

            // generate signature and set HTTP Signature headers
            var signature = signer.GetSignature();

            request.AddHeader("v-c-merchant-id", _merchantId);
            request.AddHeader("Date", signer.SignedDateTime);
            request.AddHeader("Host", uri.Host);
            request.AddHeader("Signature", signature);

            // add body digest header if necessary
            if (method == Method.POST || method == Method.PUT || method == Method.PATCH)
            {
                request.AddHeader("Digest", signer.GetDigest());
            }
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
