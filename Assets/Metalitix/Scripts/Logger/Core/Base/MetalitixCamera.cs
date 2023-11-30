using System;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class MetalitixCamera : MonoBehaviour
    {
        [SerializeField] private Transform sceneTransform;
        [SerializeField] private bool automaticallyStartLogging;
        
        public bool AutomaticallyStartLogging => automaticallyStartLogging;

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

        private void Start()
        {
            var parent = transform.parent;

            if (parent != null && parent.TryGetComponent<MetalitixScene>(out var scene))
            {
                MetalitixDebug.LogWarning(this, MetalitixRuntimeLogs.CameraElementIsParented);
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