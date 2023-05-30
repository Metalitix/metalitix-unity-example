using System;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Containers;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Converters;
using Metalitix.Scripts.Runtime.Logger.Core.Enums;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class EventPoint
    {
        public string state { get; }
        public long timestamp { get; }
        public Vector3Wrapper position { get; }

        public EventPoint(PointStates state, long timestamp, Vector3 position)
        {
            this.state = MetalitixPointStatesContainer.GetConstant(state);
            this.timestamp = timestamp;
            this.position = new Vector3Wrapper(position);
        }
    }
}