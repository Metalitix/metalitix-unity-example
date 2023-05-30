using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Kinesis
{
    [Serializable]
    public class KinesisResponseData
    {
        public string sessionId { get; }
        public int projectId { get; }
        public string dataStream { get; }
        public string accessKeyId { get; }
        public string secretKey { get; }
        public string instanceRegion { get; }
        public string createdAt { get; }
        public string updatedAt { get; }

        public KinesisResponseData() { }
            
        [JsonConstructor]
        public KinesisResponseData(string sessionId, int projectId, string dataStream, string accessKeyId, 
            string secretKey, string instanceRegion, string createdAt, string updatedAt = null)
        {
            this.sessionId = sessionId;
            this.projectId = projectId;
            this.dataStream = dataStream;
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
            this.instanceRegion = instanceRegion;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
        }
    }
}