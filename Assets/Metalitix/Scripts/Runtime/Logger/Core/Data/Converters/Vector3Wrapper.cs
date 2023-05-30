using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Converters
{
    [Serializable]
    public class Vector3Wrapper : IEquatable<Vector3Wrapper>
    {
        public float x { get; }
        public float y { get; }
        public float z { get; }

        [JsonConstructor]
        public Vector3Wrapper(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Wrapper(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ConvertToVector3()
        {
            return new Vector3(x, y, z);
        }

        public bool Equals(Vector3Wrapper other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector3Wrapper other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }
}