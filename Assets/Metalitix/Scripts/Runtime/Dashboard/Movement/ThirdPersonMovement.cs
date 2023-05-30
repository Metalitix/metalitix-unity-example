using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Movement
{
    public class ThirdPersonMovement : DashboardMovementStrategy
    {
        private readonly SceneView _sceneViewCam;
        private readonly Transform _followedObject;

        private Vector3 _positionOffset;
        private float _angle;

        private Vector3 _adjustedPos;
        private Quaternion _lastSceneViewRotation;
        
        private const float CircleRadius = 10;
        private const float ElevationOffset = 3;

        public ThirdPersonMovement(Transform followedObject, Camera targetCamera) : base(targetCamera)
        {
            _followedObject = followedObject;
            _sceneViewCam = SceneView.lastActiveSceneView;
            targetCamera.cullingMask = - 1;
        }

        public override void Move()
        {
            var camTransform = TargetCamera.transform;
            var sceneViewRotation = _sceneViewCam.camera.transform.rotation;

            Vector3 targetPosition = _followedObject.position;
            targetPosition.y += ElevationOffset;
            
            Vector3 positionOffset = new Vector3(
                Mathf.Cos(sceneViewRotation.eulerAngles.y * Mathf.Deg2Rad) * CircleRadius,
                0,
                Mathf.Sin(sceneViewRotation.eulerAngles.y * Mathf.Deg2Rad) * CircleRadius
            );
                
            camTransform.position = targetPosition + positionOffset;
            camTransform.LookAt(_followedObject);
        }
    }
}