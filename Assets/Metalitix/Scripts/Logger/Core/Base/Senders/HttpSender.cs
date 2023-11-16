using System;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Core.Tools.RequestTools;
using Newtonsoft.Json;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base.Senders
{
    public class HttpSender : DataSender<Record>
    {
        private HttpRecordWrapper _lastRecord;
        
        private readonly string _path;

        public HttpSender(GlobalSettings globalSettings, WebRequestHelper webRequestHelper) : base(globalSettings, webRequestHelper)
        {
            _path = globalSettings.ServerUrl + MetalitixConfig.DataEndPoint;
            Debug.Log(_path);
        }

        public override async Task<string> InitializeSession()
        {
            await Task.Yield();
            return Guid.NewGuid().ToString();
        }

        public override async Task SendData(Record[] data, CancellationToken token = new CancellationToken())
        {
            _lastRecord = new HttpRecordWrapper(GlobalSettings.TempApiKey, data);
            var jsonData = JsonHelper.ToJson(_lastRecord, NullValueHandling.Ignore);
            var response = await WebRequestHelper.PostDataWithPlayLoad(_path, jsonData, token);
            
            if(!string.IsNullOrEmpty(response))
                MetalitixDebug.Log(this, response);
        }
    }
}