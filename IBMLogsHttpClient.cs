using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog.Sinks.Http;

namespace Serilog.Sinks.IBMLogs
{
    class IBMLogsHttpClient : IHttpClient, IDisposable
    {
        private const string JSON_CONTENT_TYPE = "application/json";

        private readonly HttpClient _client;
        private readonly string _apikey;

        public IBMLogsHttpClient(string apiKey)
        {
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));

            _apikey = apiKey;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", JSON_CONTENT_TYPE);
        }

        public void Dispose() => _client?.Dispose();

        public void Configure(IConfiguration configuration) {}

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            HttpResponseMessage result;
            using (var content = new StreamContent(contentStream))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(JSON_CONTENT_TYPE);
                result = await _client.PostAsync(requestUri, content).ConfigureAwait(false);

                if (result.StatusCode == System.Net.HttpStatusCode.Forbidden 
                    && await AuthorizeAsync().ConfigureAwait(false) && contentStream.CanSeek)
                {
                    result.Dispose();
                    contentStream.Seek(0, SeekOrigin.Begin);
                    result = await _client.PostAsync(requestUri, content).ConfigureAwait(false);
                }
            }
            //var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            return result;
        }

        private async Task<bool> AuthorizeAsync()
        {
            using (var authClient = new HttpClient())
            {
                authClient.BaseAddress = new Uri("https://iam.cloud.ibm.com/identity/token");
                authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JSON_CONTENT_TYPE));
                authClient.Timeout = TimeSpan.FromSeconds(5);

                using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                        new KeyValuePair<string, string>("grant_type", "urn:ibm:params:oauth:grant-type:apikey"),
                        new KeyValuePair<string, string>("apikey", _apikey)
                    }))
                {
                    var request = new HttpRequestMessage { Method = HttpMethod.Post, Content = content };
                    var response = await authClient.SendAsync(request).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) return false;

                    var credentials = JsonConvert.DeserializeObject<CloudantCredentials>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    lock (_client)
                    {
                        _client.DefaultRequestHeaders.Remove("Authorization");
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(credentials.TokenType, credentials.AccessToken);
                    }
                    return true;
                }
            }
        }
    }

    internal class CloudantCredentials
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }    
}
