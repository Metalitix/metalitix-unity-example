using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class CameraViewDrawer : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        private MetalitixCameraData _cameraData;

        public void Initialize()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void UpdateCameraData(MetalitixCameraData cameraData)
        {
            _cameraData = cameraData;
        }

        public void UpdateView()
        {
            if (_lineRenderer == null || _cameraData == null) return;
            
            var nearPlane = _cameraData.zNearPlane;
            var farPlane = _cameraData.zFarPlane;

            if (farPlane > 100)
            {
                farPlane = 100;
            }
            
            var fov = _cameraData.fieldOfView;
            var aspect = _cameraData.aspectRatio;
            var tanFov = Mathf.Tan(fov * Mathf.Deg2Rad / 2f);
            var nearHeight = 2f * tanFov * nearPlane;
            var nearWidth = nearHeight * aspect;
            var farHeight = 2f * tanFov * farPlane;
            var farWidth = farHeight * aspect;
            
            Vector3 localPos = transform.localPosition;
            Vector3 topLeftNear = localPos + new Vector3(-nearWidth / 2f, nearHeight / 2f, nearPlane);
            Vector3 topRightNear = localPos + new Vector3(nearWidth / 2f, nearHeight / 2f, nearPlane);
            Vector3 bottomRightNear = localPos + new Vector3(nearWidth / 2f, -nearHeight / 2f, nearPlane);
            Vector3 bottomLeftNear = localPos + new Vector3(-nearWidth / 2f, -nearHeight / 2f, nearPlane);
            Vector3 topLeftFar = localPos + new Vector3(-farWidth / 2f, farHeight / 2f, farPlane);
            Vector3 topRightFar = localPos + new Vector3(farWidth / 2f, farHeight / 2f, farPlane);
            Vector3 bottomRightFar = localPos + new Vector3(farWidth / 2f, -farHeight / 2f, farPlane);
            Vector3 bottomLeftFar = localPos + new Vector3(-farWidth / 2f, -farHeight / 2f, farPlane);

            _lineRenderer.positionCount = 18;
            _lineRenderer.SetPositions(new Vector3[]
            {
                topLeftNear, topRightNear, topRightFar, topLeftFar, topLeftNear,
                topRightNear, bottomRightNear, bottomRightFar, topRightFar, topRightNear,
                bottomRightNear, bottomLeftNear, topLeftNear, bottomLeftNear, bottomLeftFar,
                topLeftFar, bottomLeftFar, bottomRightFar
            });
        }
    }
}