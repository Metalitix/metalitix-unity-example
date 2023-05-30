using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools
{
    [ExecuteInEditMode]
    public class LookAtDashboardCamera : MonoBehaviour
    {
        private DashboardCamera _dashboardCamera;

        private const float RotatingSpeed = 7f;
        
        public void Initialize(DashboardCamera dashboardCamera)
        {
            _dashboardCamera = dashboardCamera;
        }
        
        private void Update()
        {
            if(_dashboardCamera == null) return;
            
            var lookPos = transform.position - _dashboardCamera.transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotatingSpeed);
        }
    }
}