using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class BuildData
    {
        public float version { get; }
        public string buildType { get; }
        public string uploadedFrom { get; private set; }
        public long buildDataCreatedTime { get; private set; }
        public string buildGuid { get; private set; }
        
        [JsonConstructor]
        public BuildData(float version, string buildType)
        {
            this.version = version;
            this.buildType = buildType;

            buildGuid = Guid.NewGuid().ToString();
            buildDataCreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public BuildData(float version, string buildType, string uploadedFrom)
        {
            this.version = version;
            this.buildType = buildType;
            this.uploadedFrom = uploadedFrom;
            
            buildGuid = Guid.NewGuid().ToString();
            buildDataCreatedTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void SetUrl(string url)
        {
            uploadedFrom = url;
        }
    }
}