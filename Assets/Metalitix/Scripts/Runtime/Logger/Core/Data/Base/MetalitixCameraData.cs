using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetalitixCameraData
    {
        public float fieldOfView { get; }
        public float aspectRatio { get; }
        public float zNearPlane { get; }
        public float zFarPlane { get; }

        public MetalitixCameraData(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError("Camera is null");
                return;
            }
            
            fieldOfView = Mathf.RoundToInt(camera.fieldOfView);
            aspectRatio = Mathf.RoundToInt(camera.aspect);
            zNearPlane = Mathf.RoundToInt(camera.nearClipPlane);
            zFarPlane = Mathf.RoundToInt(camera.farClipPlane);
            
            if (zNearPlane == 0)
            {
                zNearPlane = 1;
            }
        }
        
        [JsonConstructor]
        public MetalitixCameraData(float fieldOfView, float aspectRatio, float zNearPlane, float zFarPlane)
        {
            this.fieldOfView = fieldOfView;
            this.aspectRatio = aspectRatio;
            this.zNearPlane = zNearPlane;
            this.zFarPlane = zFarPlane;
        }
    }
}