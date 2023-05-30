using Metalitix.Scripts.Extensions;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Base
{
    public class TrackingEntity : MonoBehaviour
    {
        [SerializeField] private Transform sceneTransform;

        public Vector3 Position
        {
            get
            {
                if(sceneTransform == null)
                   return transform.position.MetalitixPosition();
                
                var entityMatrixRelativeToTransform = GetRelativeMatrix();
                Vector3 position = entityMatrixRelativeToTransform.GetColumn(3);
                return position.MetalitixPosition();
            }
        }

        public Vector3 Direction
        {
            get
            {
                if(sceneTransform == null)
                    return transform.rotation.MetalitixDirection();
                
                var entityMatrixRelativeToTransform = GetRelativeMatrix();
                Quaternion rotation = Quaternion.LookRotation(entityMatrixRelativeToTransform.GetColumn(2), 
                    entityMatrixRelativeToTransform.GetColumn(1));
                return rotation.MetalitixDirection();
            }
        }

        private Matrix4x4 GetRelativeMatrix()
        {
            Matrix4x4 sceneMatrixInverse = sceneTransform.worldToLocalMatrix;
            Matrix4x4 entityMatrix = transform.localToWorldMatrix;
            Matrix4x4 entityMatrixRelativeToTransform = sceneMatrixInverse * entityMatrix;
            return entityMatrixRelativeToTransform;
        }
    }
}