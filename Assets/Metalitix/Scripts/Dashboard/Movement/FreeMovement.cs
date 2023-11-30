
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Movement
{
    public class FreeMovement : DashboardMovementStrategy
    {
#if UNITY_EDITOR
        private readonly SceneView _sceneViewCam;
#endif

        public FreeMovement(Camera targetCamera) : base(targetCamera)
        {
#if UNITY_EDITOR
            _sceneViewCam = SceneView.lastActiveSceneView;
#endif
            targetCamera.cullingMask = -1;
        }

        public override void Move()
        {
#if UNITY_EDITOR
            var camTransform = TargetCamera.transform;
            var currentPos = camTransform.position;
            var currentRot = camTransform.rotation;

            camTransform.position =
                Vector3.Lerp(currentPos, _sceneViewCam.camera.transform.position, Time.deltaTime * LerpSpeed);
            camTransform.rotation =
                Quaternion.Lerp(currentRot, _sceneViewCam.camera.transform.rotation, Time.deltaTime * LerpSpeed);
#endif
        }
    }
}