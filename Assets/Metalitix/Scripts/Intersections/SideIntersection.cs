using System.Collections.Generic;
using UnityEngine;

namespace Metalitix.Scripts.Intersections
{
    public class SideIntersection : IntersectionAlgorithm
    {
        public SideIntersection(Bounds bounds, PhysicsScene physicsScene) : base(bounds, physicsScene) { }

        public override List<Vector4> Intersect()
        {
            for (var z = Bounds.min.z; z <= Bounds.max.z; z += RayCastStepY) 
            {
                for (var y = Bounds.min.y + RayCastStepY / 2; y <= Bounds.max.y; y += RayCastStepY) 
                {
                    var start = new Vector3(Bounds.min.x, y, z);
                    var end = new Vector3(Bounds.max.x, y, z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }
            
            return Hits;
        }

        public override List<Vector4> ViseVersaIntersect()
        {
            Intersect();

            for (var z = Bounds.min.z; z <= Bounds.max.z; z += RayCastStepY) 
            {
                for (var y = Bounds.min.y + RayCastStepY / 2; y <= Bounds.max.y; y += RayCastStepY) 
                {
                    var start = new Vector3(Bounds.max.x, y, z);
                    var end = new Vector3(Bounds.min.x, y, z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }

            return Hits;
        }
    }
}