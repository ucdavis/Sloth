
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace Sloth.Integrations.Cybersource.Helpers
{
    public class SignatureRequest
    {
        private readonly string SignatureAlgorithm = "HmacSHA256";

        
        public SignatureRequest()
        {
            SignedDateTime = DateTime.UtcNow.ToString("r");
        }

        public string SignedDateTime { get; set; }

        public string MerchantId { get; set; }

        public string KeyId { get; set; }

        public string KeySecret { get; set; }

        public string JsonBody { get; set; }

        public Method Method { get; set; }

        public string HostName { get; set; }

        public Uri TargetUri { get; set; }

        private string GetHttpSignRequestTarget()
        {
            return Method.ToString().ToLower() + " " + Uri.EscapeUriString(TargetUri.PathAndQuery);
        }

        public string GetDigest()
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(JsonBody)));
                return $"SHA-256={hash}";
            }
        }

        public string GetSignature()
        {
            if (Method == Method.GET || Method == Method.DELETE)
            {
                return SignatureWithoutBody();
            }

            if (Method == Method.POST || Method == Method.PUT || Method == Method.PATCH)
            {
                return SignatureWithBody();
            }

            return string.Empty;
        }

        private string SignatureWithoutBody()
        {
            var signedHeaders = new List<string>
            {
                $"host: {HostName}",
                $"date: {SignedDateTime}",
                $"(request-target): {GetHttpSignRequestTarget()}",
                $"v-c-merchant-id: {MerchantId}"
            };
            var payload = string.Join("\n", signedHeaders);

            var hash = new HMACSHA256(Convert.FromBase64String(KeySecret)).ComputeHash(Encoding.UTF8.GetBytes(payload));
            var base64String = Convert.ToBase64String(hash);

            return
                $"keyid=\"{KeyId}\"" +
                $", algorithm=\"{SignatureAlgorithm}\"" +
                $", headers=\"host date (request-target) v-c-merchant-id\"" +
                $", signature=\"{base64String}\"";
        }

        private string SignatureWithBody()
        {
            var digest = GetDigest();

            var signedHeaders = new List<string>
            {
                $"host: {HostName}",
                $"date: {SignedDateTime}",
                $"(request-target): {GetHttpSignRequestTarget()}",
                $"digest: {digest}",
                $"v-c-merchant-id: {MerchantId}"
            };
            var payload = string.Join("\n", signedHeaders);

            var hash = new HMACSHA256(Convert.FromBase64String(KeySecret)).ComputeHash(Encoding.UTF8.GetBytes(payload));
            var base64String = Convert.ToBase64String(hash);

            return
                $"keyid=\"{KeyId}\"" +
                $", algorithm=\"{SignatureAlgorithm}\"" +
                $", headers=\"host date (request-target) digest v-c-merchant-id\"" +
                $", signature=\"{base64String}\"";
        }
    }
}
