using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class Record
    {
        public string eventType { get; }
        public string sessionId { get; }
        public DateTime  timestamp { get; }
        public string apiver { get; }

        public MetricsData metrics { get; private set;}
        public AnimationData[] animations { get; private set; }
        public MetalitixUserEvent userEvent { get; private set; }
        public MetalitixUserMetaData userMeta { get; private set; }
        public MetalitixCameraData camera { get; private set; }
        public MetalitixTrackingData data { get; private set; }

        [JsonConstructor]
        public Record(string eventType, string sessionId, DateTime  timestamp, string apiver, MetricsData metrics, 
            AnimationData[] animations, MetalitixUserEvent userEvent, MetalitixUserMetaData userMeta, 
            MetalitixCameraData camera, MetalitixTrackingData data)
        {
            this.eventType = eventType;
            this.sessionId = sessionId;
            this.timestamp = timestamp;
            this.apiver = apiver;
            this.metrics = metrics;
            this.animations = animations;
            this.userEvent = userEvent;
            this.userMeta = userMeta;
            this.camera = camera;
            this.data = data;
        }

        public Record(string eventType, string sessionId, DateTime  timestamp, MetalitixTrackingData data)
        {
            this.eventType = eventType;
            this.sessionId = sessionId;
            this.timestamp = timestamp;
            this.data = data;
            apiver = "v2";
        }

        public void SetMetrics(MetricsData metricsData)
        {
            metrics = metricsData;
        }

        public void SetAnimations(AnimationData[] animationData)
        {
            animations = animationData;
        }

        public void SetUserEvent(MetalitixUserEvent metalitixUserEvent)
        {
            userEvent = metalitixUserEvent;
        }
        
        public void SetMetaData(MetalitixUserMetaData metalitixUserMetaData)
        {
            userMeta = metalitixUserMetaData;
        }
        
        public void SetCameraData(MetalitixCameraData metalitixCameraData)
        {
            camera = metalitixCameraData;
        }

        public long GetUnixTimestamp()
        {
            return ((DateTimeOffset)timestamp).ToUnixTimeSeconds();
        }
    }
}