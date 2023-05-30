using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Data
{
    public class PointPower
    {
        public Vector3 Position { get; }
        public float Power { get; private set; }
        public float DensityFactor { get; }

        public PointPower(Vector3 position, float power, float densityFactor)
        {
            Position = position;
            Power = power;
            DensityFactor = densityFactor;
        }

        public float IncreasePower()
        {
            Power += 1f;
            return Power;
        }

        public void SetPower(float power)
        {
            Power = power;
        }
    }
}