using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Movement
{
    public abstract class DashboardMovementStrategy
    {
        protected readonly Camera TargetCamera;
        protected bool Lock;
        protected const float LerpSpeed = 4f;

        protected virtual void ChangeLock(bool state)
        {
            Lock = state;
        }

        protected DashboardMovementStrategy(Camera targetCamera)
        {
            TargetCamera = targetCamera;
        }

        public abstract void Move();
    }
}