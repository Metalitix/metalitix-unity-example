using System;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetadataSystemInfo
    {
        public string deviceModel { get; }    
        public string deviceType { get; }  
        public string operatingSystemName { get; }  
        public string operatingSystemVersion { get; }

        public MetadataSystemInfo(string deviceModel, string deviceType, string operatingSystemName, 
            string operatingSystemVersion)
        {
            this.deviceModel = deviceModel;
            this.deviceType = deviceType;
            this.operatingSystemName = operatingSystemName;
            this.operatingSystemVersion = operatingSystemVersion;
        }
    }
}