using System;
using System.Globalization;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SphereCollider))]
    public class PathPoint : PreviewObject<MeshRenderer, SphereCollider>
    {
        protected Vector3 Position;
        protected Vector3 Direction;
        protected DateTime Timestamp;
        protected string EventName;
        protected int ID;
        protected int? EventID;
        protected MetalitixCameraData MetalitixCameraData;
        
        public event Action<PathPoint> OnInteract; 

        public int GetID => ID;
        public int? GetEventID => EventID;
        public string GetDate => Timestamp.ToString("dd MMMM yyyy h:mm tt", CultureInfo.GetCultureInfo("en-GB"));
        public DateTime GetTimeStamp => Timestamp;
        public long GetUnixTimeStamp => DateTimeToUnix(Timestamp);
        public Vector3 GetPosition => Position;
        public Vector3 GetDirection => Direction;
        public string GetEventName => EventName;
        public MetalitixCameraData GetMetalitixCameraData => MetalitixCameraData;

        public virtual void Initialize(int id, DateTime timestamp, Vector3 position, Vector3 rotation, MetalitixCameraData metalitixCameraData, int? eventID = null, string eventName = null)
        {
            ID = id;
            EventID = eventID;
            Position = position;
            Direction = rotation;
            EventName = eventName;
            Timestamp = timestamp;
            transform.position = Position;
            MetalitixCameraData = metalitixCameraData;
        }
        
        public override void Interact()
        {
            OnInteract?.Invoke(this);
        }
        
        private long DateTimeToUnix(DateTime dt)
        {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dt, TimeSpan.Zero);
            long unixTimestamp = dateTimeOffset.ToUnixTimeSeconds();
            return unixTimestamp;
        }
    }
}