using Metalitix.Scripts.Dashboard.Base;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Tools
{
    [ExecuteInEditMode]
    public class ScaleWithDashboardCameraDistance : MonoBehaviour
    {
        private DashboardCamera _dashboardCamera;
        private Transform _targetTransform;

        private float _minScale;
        private float _maxScale;

        private const float ScaleOffset = 20f;
        private const float MinDistance = 2f;
        private const float MaxDistance = 35f;

        public void Initialize(Transform targetTransform, DashboardCamera dashboardCamera)
        {
            _targetTransform = targetTransform;
            _dashboardCamera = dashboardCamera;

            _minScale = _targetTransform.localScale.x;
            _maxScale = _targetTransform.localScale.x * ScaleOffset;
        }

        private void Update()
        {
            if (_dashboardCamera == null) return;

            var distance = Vector3.Distance(_dashboardCamera.transform.position, _targetTransform.position);
            var scale = Mathf.Lerp(_minScale, _maxScale, Mathf.InverseLerp(MinDistance, MaxDistance, distance));
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}