using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Movement
{
    public class FirstPersonMovement : DashboardMovementStrategy
    {
        private readonly Transform _followedObject;
        
        private const string PreviewLayer = "Preview";

        public FirstPersonMovement(Transform targetTransform, Camera targetCamera) : base(targetCamera)
        {
            _followedObject = targetTransform;
            int layerMask = -1;
            layerMask &= ~(1 << LayerMask.NameToLayer(PreviewLayer));
            targetCamera.cullingMask = layerMask;
        }

        public override void Move()
        {
            var camTransform = TargetCamera.transform;
            camTransform.position = _followedObject.position;
            camTransform.rotation = _followedObject.rotation;
        }
    }
}