using System;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetalitixTrackingData : IEquatable<MetalitixTrackingData>
    {
        public Vector3Wrapper position { get; private set; }
        public Vector3Wrapper direction { get; private set; }
        public bool local { get; private set; }
        public string eulerOrder { get; private set; }

        [JsonExtensionData]
        public JObject customFields { get; private set; }
        
        public MetalitixTrackingData(Vector3 position, Vector3 direction)
        {
            this.position = new Vector3Wrapper(position);
            this.direction = new Vector3Wrapper(direction);
            local = true;
            eulerOrder = "YXZ";
            customFields = new JObject();
        }

        public void SetFields(JObject fields)
        {
            customFields = fields;
        }

        public bool Equals(MetalitixTrackingData other)
        {
            return position.Equals(other.position) && direction.Equals(other.direction);
        }

        public override bool Equals(object obj)
        {
            return obj is MetalitixTrackingData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, direction, customFields);
        }
    }
}