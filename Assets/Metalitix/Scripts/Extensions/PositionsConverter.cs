using System;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Converters;
using UnityEngine;

namespace Metalitix.Scripts.Extensions
{
    public static class PositionsConverter
    {
        public static Vector3 MetalitixPosition(this Vector3 unityCoordinates)
        {
            return new Vector3(-unityCoordinates.x, unityCoordinates.y, unityCoordinates.z);
        }
        
        public static Vector3 MetalitixPosition(this Vector3Wrapper unityCoordinates)
        {
            return new Vector3(-unityCoordinates.x, unityCoordinates.y, unityCoordinates.z);
        }
        
        // https://stackoverflow.com/a/53016486
        // https://stackoverflow.com/questions/18066581/convert-unity-transforms-to-three-js-rotations
        public static Vector3 MetalitixDirection(this Quaternion unityQuaternion)
        {
            var euler = unityQuaternion.eulerAngles;
        
            var convertedEuler = new Vector3(
                euler.x * Mathf.Deg2Rad,
                euler.y * Mathf.Deg2Rad,
                euler.z * Mathf.Deg2Rad);

            convertedEuler.y += Mathf.PI / 2;
            return convertedEuler;
        }

        // https://stackoverflow.com/a/53016486
        // https://stackoverflow.com/questions/18066581/convert-unity-transforms-to-three-js-rotations
        public static Vector3 ConvertToUnityDirection(this Vector3Wrapper jsEuler)
        {
            var jsEulerVector = jsEuler.ConvertToVector3();
            jsEulerVector.y -= Mathf.PI / 2;

            var convertedEuler = new Vector3(
                jsEulerVector.x * Mathf.Rad2Deg,
                jsEulerVector.y * Mathf.Rad2Deg,
                jsEulerVector.z * Mathf.Rad2Deg);
                    
            return convertedEuler;
        }
    }
    
    public enum TypeOfConversion
    {
        HeatMap,
        Position
    }
}