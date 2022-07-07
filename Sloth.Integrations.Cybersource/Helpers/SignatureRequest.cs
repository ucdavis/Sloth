
using System;
using System.Collections.Generic;
using System.Linq;
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

        public string KeySecret { private get; set; }

        public string JsonBody { get; set; }

        public Method Method { get; set; }

        public string HostName { get; set; }

        public Uri TargetUri { get; set; }

        private string GetHttpSignRequestTarget()
        {
            // TODO: EscapeUriString is obsolete -- need to determine format of TargetUri and try to use that directly instead
            return Method.ToString().ToLower() + " " + Uri.EscapeUriString(TargetUri.PathAndQuery);
        }

        private string _cachedDigest;
        public string GetDigest()
        {
            if (!string.IsNullOrWhiteSpace(_cachedDigest))
            {
                return _cachedDigest;
            }

            using (var sha256 = SHA256.Create())
            {
                var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(JsonBody)));
                _cachedDigest = $"SHA-256={hash}";
                return _cachedDigest;
            }
        }

        public string GetSignature()
        {
            // create a list of all the headers we're going to sign
            // format:
            //  "key: value"
            //      with newlines between each value pair
            var signedHeaders = GetSignedHeaders();
            var payloadList = signedHeaders.Select(h => $"{h.Key}: {h.Value}");
            var payload = string.Join("\n", payloadList);

            // sign the headers
            var hash = new HMACSHA256(Convert.FromBase64String(KeySecret)).ComputeHash(Encoding.UTF8.GetBytes(payload));
            var base64String = Convert.ToBase64String(hash);

            // format the list for the signature as "header header"
            var headersList = signedHeaders.Select(h => h.Key);
            var headers = string.Join(" ", headersList);

            return
                $"keyid=\"{KeyId}\"" +
                $", algorithm=\"{SignatureAlgorithm}\"" +
                $", headers=\"{headers}\"" +
                $", signature=\"{base64String}\"";
        }

        private Dictionary<string, string> GetSignedHeaders()
        {
            var result = new Dictionary<string, string>()
            {
                { "host", HostName },
                { "date", SignedDateTime },
                // the target isn't a real header, so apparently we wrap it in parenthesis
                { "(request-target)", GetHttpSignRequestTarget() },
                { "v-c-merchant-id", MerchantId },
            };

            // include the digest if necessary
            if (Method == Method.Post || Method == Method.Put || Method == Method.Patch)
            {
                var digest = GetDigest();
                result.Add("digest", digest);
            }

            return result;
        }
    }
}
