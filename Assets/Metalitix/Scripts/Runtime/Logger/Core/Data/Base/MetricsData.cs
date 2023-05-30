using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetricsData
    {
        public int fps { get; private set; }

        public MetricsData() { }
        
        [JsonConstructor]
        public MetricsData(int fps)
        {
            this.fps = fps;
        }

        public void SetFps(int fps)
        {
            this.fps = fps;
        }
    }
}