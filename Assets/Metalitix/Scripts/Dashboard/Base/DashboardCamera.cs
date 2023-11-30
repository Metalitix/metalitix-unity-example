using System;
using Metalitix.Scripts.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Base
{
    [ExecuteInEditMode]
    public class DashboardCamera : MonoBehaviour
    {
        private Camera _targetRenderCamera;
        private Transform _followedObject;
        private DashboardMovementStrategy _movementStrategy;
        
        public Camera TargetRenderCamera => _targetRenderCamera;

        public void Initialize()
        {
            _targetRenderCamera = GetComponent<Camera>();
            SetStrategy(DashboardMovementType.Free);
        }

        public void SetObjectToFollow(Transform targetTransform)
        {
            _followedObject = targetTransform;
        }

        public void SetStrategy(DashboardMovementType movementType)
        {
            if (_followedObject != null)
                _followedObject.gameObject.SetActive(true);

            switch (movementType)
            {
                case DashboardMovementType.Free:
                    _movementStrategy = new FreeMovement(_targetRenderCamera);
                    break;
                case DashboardMovementType.FirstPerson:
                    _followedObject.gameObject.SetActive(false);
                    _movementStrategy = new FirstPersonMovement(_followedObject, _targetRenderCamera);
                    break;
                case DashboardMovementType.ThirdPerson:
                    _movementStrategy = new ThirdPersonMovement(_followedObject, _targetRenderCamera);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(movementType), movementType, null);
            }
        }

        private void Update()
        {
            if (_movementStrategy != null)
                _movementStrategy.Move();
        }
    }
}