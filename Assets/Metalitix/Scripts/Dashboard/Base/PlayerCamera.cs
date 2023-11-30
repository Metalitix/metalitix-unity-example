using Metalitix.Core.Data.Runtime;
using Metalitix.Scripts.Dashboard.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Base
{
    [ExecuteInEditMode]
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private CameraViewDrawer cameraViewDrawer;

        private DashboardCamera _camera;

        public void Initialize(DashboardCamera camera)
        {
            _camera = camera;
            cameraViewDrawer.Initialize();
        }

        public void SetVisible(bool state)
        {
            gameObject.SetActive(state);
        }

        public void SetData(Vector3 position, Vector3 direction, MetalitixCameraData metalitixCameraData)
        {
            transform.position = position;
            transform.localRotation = Quaternion.Euler(direction);
            cameraViewDrawer.UpdateCameraData(metalitixCameraData);
            cameraViewDrawer.UpdateView();
        }
    }
}