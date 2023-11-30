
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Movement
{
    public class ThirdPersonMovement : DashboardMovementStrategy
    {
#if UNITY_EDITOR
        private readonly SceneView _sceneViewCam;
#endif
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
#if UNITY_EDITOR
            _sceneViewCam = SceneView.lastActiveSceneView;
#endif
            targetCamera.cullingMask = -1;
        }

        public override void Move()
        {
#if UNITY_EDITOR
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
#endif
        }
    }
}