using System.Collections.Generic;
using UnityEngine;

namespace Metalitix.Scripts.Intersections
{
    public class VerticalIntersection : IntersectionAlgorithm
    {
        public VerticalIntersection(Bounds bounds, PhysicsScene physicsScene) : base(bounds, physicsScene) { }

        public override List<Vector4> Intersect()
        {
            for (var x = Bounds.min.x + RayCastStepX / 2; x <= Bounds.max.x; x += RayCastStepX) 
            {
                for (var z = Bounds.min.z + RayCastStepZ / 2; z <= Bounds.max.z; z += RayCastStepZ) 
                {
                    var start = new Vector3(x, Bounds.min.y, z);
                    var end = new Vector3(x, Bounds.max.x, z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }
            
            return Hits;
        }

        public override List<Vector4> ViseVersaIntersect()
        {
            Intersect();

            for (var x = Bounds.min.x + RayCastStepX / 2; x <= Bounds.max.x; x += RayCastStepX) 
            {
                for (var z = Bounds.min.z + RayCastStepZ / 2; z <= Bounds.max.z; z += RayCastStepZ) 
                {
                    var start = new Vector3(x, Bounds.max.y, z);
                    var end = new Vector3(x, Bounds.min.x, z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }
            
            return Hits;
        }
    }
}