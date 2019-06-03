using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ZephyrCloudHelper
{
    /// <summary>
    /// Zephyr cloud API client
    /// </summary>
    internal class ZapiClient
    {
        private const int ExpireTime = 3600;
        
        private readonly RestClient _restClient;

        private readonly string _user;
        private readonly string _accessKey;
        private readonly string _secretKey;

        public ZapiClient(string user, string accessKey, string secretKey, string baseUrl)
        {
            _user = user;
            _accessKey = accessKey;
            _secretKey = secretKey;

            _restClient = new RestClient(baseUrl);
        }

        public JContainer Get(string relativePath, string queryString = null)
        {
            return SendHttpRequest(relativePath, queryString, null, Method.GET);
        }

        public JContainer Delete(string relativePath, string queryString = null)
        {
            return SendHttpRequest(relativePath, queryString, null, Method.DELETE);
        }

        public JContainer Put(string relativePath, object requestPayload)
        {
            return SendHttpRequest(relativePath, null, requestPayload, Method.PUT);
        }

        public JContainer Post(string relativePath, object requestPayload)
        {
            return SendHttpRequest(relativePath, null, requestPayload, Method.POST);
        }

        public JContainer Post(string relativePath, string queryString, object requestPayload)
        {
            return SendHttpRequest(relativePath, queryString, requestPayload, Method.POST);
        }

        public JContainer SendHttpRequest(string relativePath, string queryString, object requestPayload, Method method)
        {

            var utc0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var issueTime = DateTime.Now;

            var iat = (long) issueTime.Subtract(utc0).TotalMilliseconds;
            var exp = (long) issueTime.AddMilliseconds(ExpireTime).Subtract(utc0).TotalMilliseconds;

            //example:
            //relativePath = "/public/rest/api/1.0/cycles/search";
            //queryString = "expand=executionSummaries&projectId=10000&versionId=-1";
            string canonicalPath = null;
            if (method == Method.GET) canonicalPath = $"GET&{relativePath}&{queryString}";
            if (method == Method.DELETE) canonicalPath = $"DELETE&{relativePath}&{queryString}";
            if (method == Method.PUT) canonicalPath = $"PUT&{relativePath}&{queryString}";
            if (method == Method.POST) canonicalPath = $"POST&{relativePath}&{queryString}";

            var payload = new Dictionary<string, object>
            {
                {"sub", _user}, //assign subject 
                {"qsh", GetQSH(canonicalPath)}, //assign query string hash
                {"iss", _accessKey}, //assign issuer
                {"iat", iat}, //assign issue at(in ms)
                {"exp", exp} //assign expiry time(in ms)
            };

            var algorithm = new HMACSHA256Algorithm();
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, _secretKey);
            IRestResponse response = null;

            try
            {
                var source = string.IsNullOrWhiteSpace(queryString)
                    ? $"{relativePath}"
                    : $"{relativePath}?{queryString}";

                var retryTimes = 1;
                Retry: // forgive my dull please but need to hack the Zephyr JWT by using retry
                var request = new RestRequest(source, method);
                if (method == Method.PUT || method == Method.POST)
                    request.AddJsonBody(requestPayload);

                request.AddHeader("Authorization", "JWT " + token).AddHeader("zapiAccessKey", _accessKey);

                response = _restClient.Execute(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (!string.IsNullOrWhiteSpace(response.Content) && response.Content.Contains("qsh"))
                    {
                        var matches = Regex.Match(response.Content, @"'\w{4,}'");
                        if (matches.Success)
                        {
                            payload["qsh"] = matches.Groups[0].Value.Trim('\'');
                            token = encoder.Encode(payload, _secretKey);
                            if (retryTimes-- > 0)
                                goto Retry;
                        }
                    }
                }

                var data = (JContainer) JToken.Parse(response.Content);

                return data;
            }
            catch (Exception e)
            {
                Debug.WriteLine(
                    $"ZAPI - The http status is:{response?.StatusCode} for {relativePath}\\{requestPayload}\nThe exception is:{e.Message}");
                if (response?.StatusCode == HttpStatusCode.OK
                    || response?.StatusCode == HttpStatusCode.Created
                    || response?.StatusCode == HttpStatusCode.Accepted
                    || response?.StatusCode == HttpStatusCode.NoContent)
                    return new JConstructor("content", response.Content);

                return null;
            }
        }

        private string GetQSH(string qstring)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(qstring), 0, Encoding.UTF8.GetByteCount(qstring));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}
