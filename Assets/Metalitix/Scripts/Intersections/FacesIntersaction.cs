using System.Collections.Generic;
using UnityEngine;

namespace Metalitix.Scripts.Intersections
{
    public class FacesIntersection : IntersectionAlgorithm
    {
        public FacesIntersection(Bounds bounds, PhysicsScene physicsScene) : base(bounds, physicsScene) { }

        public override List<Vector4> Intersect()
        {
            for (var x = Bounds.min.x; x <= Bounds.max.x; x += RayCastStepX) 
            {
                for (var y = Bounds.min.y; y <= Bounds.max.y; y += RayCastStepY) 
                {
                    var start = new Vector3(x, y, Bounds.min.z);
                    var end = new Vector3(x, y, Bounds.max.z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }
            
            return Hits;
        }

        public override List<Vector4> ViseVersaIntersect()
        {
            Intersect();
            
            for (var x = Bounds.min.x; x <= Bounds.max.x; x += RayCastStepX) 
            {
                for (var y = Bounds.min.y; y <= Bounds.max.y; y += RayCastStepY) 
                {
                    var start = new Vector3(x, y, Bounds.max.z);
                    var end = new Vector3(x, y, Bounds.min.z);

                    var direction = end - start;
                    ThrowRay(direction.normalized,start);
                }
            }

            return Hits;
        }
    }
}