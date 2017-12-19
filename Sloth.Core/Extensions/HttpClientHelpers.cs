using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sloth.Core.Extensions
{
    public static class HttpClientHelpers
    {
        public static StringContent GetJsonContent(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }

        public static async Task<T> GetContentOrNullAsync<T>(this HttpResponseMessage response)
        {
            // return null on 404
            if (response.StatusCode == HttpStatusCode.NotFound)
                return default(T);

            // check for success
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<T>();

            // package other codes
            throw await PackageUnsuccessfulResponse(response);
        }

        /// <summary>
        /// returns content as string or empty on 404
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<string> GetContentOrEmptyAsync(this HttpResponseMessage response)
        {
            // return empty on 404
            if (response.StatusCode == HttpStatusCode.NotFound)
                return "";

            // check for success
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            throw await PackageUnsuccessfulResponse(response);
        }

        private static async Task<HttpServiceInternalException> PackageUnsuccessfulResponse(HttpResponseMessage response)
        {
            // try to package service 500s
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var serviceError = await response.Content.ReadAsAsync<HttpServiceInternalResponse>();
                if (serviceError != null)
                {
                    return new HttpServiceInternalException(serviceError.Message)
                    {
                        StatusCode = response.StatusCode,
                        ResponseCorrelationId = serviceError.CorrelationId,
                    };
                }
            }

            var content = await response.Content.ReadAsStringAsync();
            return new HttpServiceInternalException("Response returned non-successful status code: " + response.StatusCode)
            {
                StatusCode = response.StatusCode,
                Content = content
            };
        }
    }

    /*
     * Use a response class so we're not deserializing the exception in the response (this is hard^tm)
     */
    public class HttpServiceInternalResponse
    {
        /// <summary>
        /// Message of the returned response
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Correlation Id of the returned response
        /// </summary>
        public string CorrelationId { get; set; }
    }

    public class HttpServiceInternalException : Exception
    {
        /// <summary>
        /// Status Code of the returned response
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get { return (HttpStatusCode)Data["StatusCode"]; }
            set { Data["StatusCode"] = value; }
        }

        /// <summary>
        /// Correlation Id of the returned response
        /// </summary>
        public string ResponseCorrelationId
        {
            get { return (string)Data["ResponseCorrelationId"]; }
            set { Data["ResponseCorrelationId"] = value; }
        }

        /// <summary>
        /// Content of the returned response
        /// </summary>
        public string Content
        {
            get { return (string)Data["Content"] as string; }
            set { Data["Content"] = value; }
        }

        public HttpServiceInternalException(string message) : base(message)
        {
        }
    }
}
