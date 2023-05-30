using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Movement
{
    public class FreeMovement : DashboardMovementStrategy
    {
        private readonly SceneView _sceneViewCam;

        public FreeMovement(Camera targetCamera) : base(targetCamera)
        {
            _sceneViewCam = SceneView.lastActiveSceneView;
            targetCamera.cullingMask = - 1;
        }

        public override void Move()
        {
            var camTransform = TargetCamera.transform;
            var currentPos = camTransform.position;
            var currentRot = camTransform.rotation;

            camTransform.position =
                Vector3.Lerp(currentPos, _sceneViewCam.camera.transform.position, Time.deltaTime * LerpSpeed);
            camTransform.rotation =
                Quaternion.Lerp(currentRot, _sceneViewCam.camera.transform.rotation, Time.deltaTime * LerpSpeed);
        }
    }
}