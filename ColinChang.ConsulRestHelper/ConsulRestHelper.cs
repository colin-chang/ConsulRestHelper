using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Consul;
using Newtonsoft.Json;

namespace ColinChang.ConsulRestHelper
{
    public class ConsulRestHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _consulServerUrl;

        public ConsulRestHelper(HttpClient httpClient, string consulServerUrl)
        {
            _httpClient = httpClient;
            _consulServerUrl = consulServerUrl;
        }

        public async Task<RestResponseWithBody<T>> GetForEntityAsync<T>(string url,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                        requestMsg.Headers.Add(header.Key, header.Value);
                }

                requestMsg.Method = HttpMethod.Get;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                var respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponseWithBody<T>> PostForEntityAsync<T>(string url, object body = null,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = HttpMethod.Post;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> PostAsync(string url, object body = null,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = HttpMethod.Post;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        public async Task<RestResponseWithBody<T>> PutForEntityAsync<T>(String url, object body = null,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = HttpMethod.Put;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                RestResponseWithBody<T> respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> PutAsync(string url, object body = null,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = HttpMethod.Put;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                requestMsg.Content = new StringContent(JsonConvert.SerializeObject(body));
                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        public async Task<RestResponseWithBody<T>> DeleteForEntityAsync<T>(string url,
            HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = HttpMethod.Delete;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                var respEntity = await SendForEntityAsync<T>(requestMsg);
                return respEntity;
            }
        }

        public async Task<RestResponse> DeleteAsync(string url, HttpRequestHeaders requestHeaders = null)
        {
            using (var requestMsg = new HttpRequestMessage())
            {
                if (requestHeaders != null)
                {
                    foreach (var header in requestHeaders)
                    {
                        requestMsg.Headers.Add(header.Key, header.Value);
                    }
                }

                requestMsg.Method = System.Net.Http.HttpMethod.Delete;
                requestMsg.RequestUri = new Uri(await ResolveUrlAsync(url));
                var resp = await SendAsync(requestMsg);
                return resp;
            }
        }

        private async Task<string> ResolveUrlAsync(string url)
        {
            var uri = new Uri(url);
            var serviceName = uri.Host;
            var realRootUrl = await ResolveRootUrlAsync(serviceName);
            return $"{uri.Scheme}://{realRootUrl}{uri.PathAndQuery}";
        }


        private async Task<string> ResolveRootUrlAsync(string serviceName)
        {
            using (var consulClient = new ConsulClient(c => c.Address = new Uri(_consulServerUrl)))
            {
                var services = (await consulClient.Agent.Services()).Response.Values
                    .Where(s => s.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
                if (!services.Any())
                {
                    throw new ArgumentException($"can not find any instances of {serviceName}");
                }
                else
                {
                    //按照当前时钟毫秒数对可用服务个数取模策略，获得一个服务实例
                    var service = services.ElementAt(Environment.TickCount % services.Count());
                    return $"{service.Address}:{service.Port}";
                }
            }
        }

        private async Task<RestResponseWithBody<T>> SendForEntityAsync<T>(HttpRequestMessage requestMsg)
        {
            var result = await _httpClient.SendAsync(requestMsg);
            var respEntity = new RestResponseWithBody<T> {StatusCode = result.StatusCode, Headers = result.Headers};
            var bodyStr = await result.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(bodyStr))
                respEntity.Body = JsonConvert.DeserializeObject<T>(bodyStr);

            return respEntity;
        }

        private async Task<RestResponse> SendAsync(HttpRequestMessage requestMsg)
        {
            var result = await _httpClient.SendAsync(requestMsg);
            var response = new RestResponse {StatusCode = result.StatusCode, Headers = result.Headers};
            return response;
        }
    }


    public class RestResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public HttpResponseHeaders Headers { get; set; }
    }

    public class RestResponseWithBody<T> : RestResponse
    {
        public T Body { get; set; }
    }
}
