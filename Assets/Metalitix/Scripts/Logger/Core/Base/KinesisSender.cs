using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity.Model;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Kinesis;
using Metalitix.Core.Tools;
using Newtonsoft.Json;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    internal class KinesisSender : RecordsSender
    {
        private IAmazonKinesis _kinesisClient;
        private KinesisResponseData _kinesisResponseData;
        private string _kinesisRegion;
        private readonly string _serverUrl;
        
        private RegionEndpoint KinesisRegion => RegionEndpoint.GetBySystemName(_kinesisRegion);
        
        public KinesisSender(string appKeyCode, string serverUrl) : base(appKeyCode)
        {
            _serverUrl = serverUrl;
        }
        
        public override async Task<string> InitializeSession()
        {
            _kinesisResponseData = await CreateClientSession();
            return _kinesisResponseData.sessionId;
        }

        private async Task<KinesisResponseData> CreateClientSession()
        {
            KinesisAuthData kinesisAuthData = new KinesisAuthData(appKeyCode);

            var json = JsonHelper.ToJson(kinesisAuthData, NullValueHandling.Ignore);

            StringBuilder url = new StringBuilder();
            url.Append(_serverUrl);
            url.Append(MetalitixConfig.KinesisDataStream);
            url.Append(MetalitixConfig.DataEndPoint);

            var data = await Task.Run(() => 
                WebRequestHelper.PostDataWithPlayLoad<KinesisResponseData>(url.ToString(), json, new CancellationToken()));
          
            if (String.IsNullOrEmpty(data.sessionId))
            {
                return null;
            }
            
            var accessKeyId = AESHelper.AESDecrypt(data.accessKeyId);
            var secretKey = AESHelper.AESDecrypt(data.secretKey);
            
            _kinesisRegion = data.instanceRegion;
            
            AWSCredentials awsCredentials = new Credentials()
            {
                AccessKeyId = accessKeyId,
                SecretKey = secretKey,
            };

            _kinesisClient = new AmazonKinesisClient(awsCredentials, KinesisRegion);
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.KinesisClientSuccessfullyCreated);
            return data;
        }
        
        /// <summary>
        /// Kinesis PutRecord. Puts a record with the data specified
        /// in the "Record Data" Text Input Field to the stream specified in the "Stream Name"
        /// Text Input Field.
        /// </summary>
        public override async Task SendData(object data, CancellationToken token = new CancellationToken())
        {
            if (_kinesisClient != null && _kinesisResponseData != null)
            {
                // There is a problem with Apple Mac apostrophe (correct apostrophe`s is `'´, incorrect apostrophe is ’)
                var jsonData = JsonHelper.ToJson(data, NullValueHandling.Ignore);
                var bytes = Encoding.UTF8.GetBytes(jsonData);
                using var memoryStream = new MemoryStream(bytes);
                var request = new PutRecordRequest
                {
                    Data = memoryStream,
                    PartitionKey = _kinesisResponseData.sessionId,
                    StreamName = _kinesisResponseData.dataStream
                };

                try
                {
                    var response = await _kinesisClient.PutRecordAsync(request, token);
                    LogResponse(response);
                }
                catch (HttpErrorResponseException e)
                {
                    
                    MetalitixDebug.LogError(this, e.Message);
                    throw;
                }
            }
        }

        private void LogResponse(PutRecordResponse respone)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(MetalitixRuntimeLogs.RecordWasSent);
            stringBuilder.Append(respone.HttpStatusCode);

            MetalitixDebug.Log(this, stringBuilder.ToString());
        }
    }
}