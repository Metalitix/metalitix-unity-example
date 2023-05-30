using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Extensions
{
    public static class WebRequestHelper
    {
        private const string AuthorizationHeader = "Authorization";
        private const string OriginHeader = "Origin";
        private const string LocalHost = "http://localhost";
        private const string JsonMediaType = "application/json";
        
        public static async Task<T> GetData<T>(string path, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(OriginHeader, LocalHost);
            var response = await client.GetAsync(path, cancellationToken);
            
            return await TryParseData<T>(response, client);
        }

        public static async Task<T> GetDataWithToken<T>(string path, string token, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(AuthorizationHeader, token);
            client.DefaultRequestHeaders.Add(OriginHeader, LocalHost);

            try
            {
                var response = await client.GetAsync(path, cancellationToken);
                return await TryParseData<T>(response, client);
            }
            catch (Exception e)
            {
                MetalitixDebug.Log(client, e.Message, true);
                return default;
            }
        }

        public static async Task<T> PatchData<T>(string path, string token, string jsonString, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(AuthorizationHeader, token);
            client.DefaultRequestHeaders.Add(OriginHeader,LocalHost);
            var content = new StringContent(jsonString, Encoding.UTF8, JsonMediaType);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), path) { Content = content };
            
            try
            {
                var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return await TryParseData<T>(response, client);
            }
            catch (Exception e)
            {
                MetalitixDebug.Log(client, e.Message, true);
                return default;
            }
        }

        public static async Task PostDataWithPlayLoad(string path, string jsonString, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            var data = new StringContent(jsonString, Encoding.UTF8, JsonMediaType);
            client.DefaultRequestHeaders.Add(OriginHeader,LocalHost);
            var response = await client.PostAsync(path, data, cancellationToken);
            
            await TryParseData(response, client);
            
            client.Dispose();
        }

        public static async Task<T> PostDataWithPlayLoad<T>(string path, string jsonString, CancellationToken cancellationToken) where T : class
        {
            using var client = new HttpClient();
            var data = new StringContent(jsonString, Encoding.UTF8, JsonMediaType);
            client.DefaultRequestHeaders.Add(OriginHeader, LocalHost);
            
            var response = await client.PostAsync(path, data, cancellationToken);
            return await TryParseData<T>(response, client);
        }

        private static async Task TryParseData(HttpResponseMessage response, HttpClient client)
        {
            try
            {
                var result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                client.Dispose();
            }
        }
        
        private static async Task<T> TryParseData<T>(HttpResponseMessage response, HttpClient client)
        {
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var value = JsonHelper.FromJson<T>(result, NullValueHandling.Ignore);

                if (value == null)
                {
                    throw new HttpRequestException(result);
                }
                
                return value;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                client.Dispose();
            }
        }
    }
}