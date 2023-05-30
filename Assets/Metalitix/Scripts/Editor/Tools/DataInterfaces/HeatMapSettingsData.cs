using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class HeatMapSettingsData
    {
        public float cameraScaleFactor { get; }
        public float heatmapScaleFactor { get; }
        public float heatmapBrightness { get; }
        public float positionScaleFactor { get; }
        public float positionBrightness { get; }

        [JsonConstructor]
        public HeatMapSettingsData(float cameraScaleFactor, float heatmapScaleFactor, float heatmapBrightness, float positionScaleFactor, float positionBrightness)
        {
            this.cameraScaleFactor = cameraScaleFactor;
            this.heatmapScaleFactor = heatmapScaleFactor;
            this.heatmapBrightness = heatmapBrightness;
            this.positionScaleFactor = positionScaleFactor;
            this.positionBrightness = positionBrightness;
        }
    }
}