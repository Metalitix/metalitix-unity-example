using System;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetalitixUserMetaData
    {
        public string sceneName { get; }
        public MetadataSystemInfo systemInfo { get; private set;}

        public MetalitixUserMetaData(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public void SetSystemInfo(MetadataSystemInfo metadataSystemInfo)
        {
            systemInfo = metadataSystemInfo;
        }
    }
}