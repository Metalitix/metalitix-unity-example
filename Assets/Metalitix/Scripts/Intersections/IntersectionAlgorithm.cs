using System.Collections.Generic;
using UnityEngine;

namespace Metalitix.Scripts.Intersections
{
    public abstract class IntersectionAlgorithm
    {
        protected Bounds Bounds;

        protected readonly float RayCastStepX;
        protected readonly float RayCastStepY;
        protected readonly float RayCastStepZ;
        protected readonly List<Vector4> Hits = new List<Vector4>();

        private PhysicsScene _physicsScene;
        private const float RaycastsCount = 1000f;
        
        public IntersectionAlgorithm(Bounds bounds, PhysicsScene physicsScene = default)
        {
            Bounds = bounds;
            _physicsScene = physicsScene;
            var raycastCountForAxis = Mathf.Round(Mathf.Sqrt(RaycastsCount / 6));
            RayCastStepX = (bounds.max.x - bounds.min.x) / raycastCountForAxis;
            RayCastStepY = (bounds.max.y - bounds.min.y) / raycastCountForAxis;
            RayCastStepZ = (bounds.max.z - bounds.min.z) / raycastCountForAxis;
        }

        public abstract List<Vector4> Intersect();
        
        public abstract List<Vector4> ViseVersaIntersect();

        protected void ThrowRay(Vector3 direction, Vector3 position)
        {
            if (_physicsScene != default)
            {
                if (_physicsScene.Raycast(position, direction, out var hit))
                {
                    Hits.Add(hit.point);
                }
            }
        }
    }
}