using System;
using System.Net.Http;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class InternetAccessChecker
    {
        private readonly string _testUrl;

        public InternetAccessChecker(string url)
        {
            _testUrl = url;
        }
        
        public async Task<bool> CheckInternetAccess()
        {
            using HttpClient httpClient = new HttpClient();
        
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(_testUrl);

                if (response.IsSuccessStatusCode)
                {
                    MetalitixDebug.Log(this,MetalitixRuntimeLogs.InternetAccessAvailable);
                }
                else
                {
                    MetalitixDebug.LogError(this, MetalitixRuntimeLogs.HttpRequestFailed + response.StatusCode);
                }

                return true;
            }
            catch (Exception e)
            {
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.Exception + e.Message);
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.InternetConnectionError);
                return false;
            }
        }
    }
}
