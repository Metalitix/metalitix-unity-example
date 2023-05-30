using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Survey.Animation._3D
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private float smoothTime = 0.3f;

        private Transform _currentTransform;
        private Transform _camTransform;
        private Vector3 _offset;
        private Vector3 _velocity = Vector3.zero;
 
        private void Start()
        {
            _currentTransform = transform;
            
            if (Camera.main != null)
                _camTransform = Camera.main.transform;
            
            _offset = _camTransform.position - _currentTransform.position;
        }
 
        private void LateUpdate()
        {
            if(_camTransform == null) return;
            
            var cameraForward = _camTransform.forward;
            var targetPosition = _camTransform.position - _offset;
            _currentTransform.position = Vector3.SmoothDamp(_currentTransform.position, targetPosition, ref _velocity, smoothTime);
            _currentTransform.forward = cameraForward;
        }
    }
}