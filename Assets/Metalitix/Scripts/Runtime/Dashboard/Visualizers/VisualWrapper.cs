using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Visualizers
{
    [ExecuteInEditMode]
    public class VisualWrapper : MonoBehaviour
    {
        [Header("Materials")]
        [SerializeField] private Material standardHeatMapMaterial;

        private MaterialPropertyBlock _currentMaterialPropertyBlock;
        private List<Renderer> _renderers;
        private Transform _heatRenderersParent;
        private Transform _transparencyRenderersParent;
        
        private readonly int PointsID = Shader.PropertyToID("pointsData");
        private readonly int PointSizeID = Shader.PropertyToID("pointSize");
        private readonly int MaxDistanceID = Shader.PropertyToID("_MaxDistance");

        public void Visualize()
        {
            FindRenderers();
        }
        
        private void FindRenderers()
        {
            _renderers = new List<Renderer>();
            var skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

            if (skinnedMeshes.Length > 0)
            {
                foreach (var skinnedMesh in skinnedMeshes)
                {
                    var collider = skinnedMesh.AddComponent<MeshCollider>();
                    collider.sharedMesh = skinnedMesh.sharedMesh;
                    skinnedMesh.sharedMaterial = standardHeatMapMaterial;
                    _renderers.Add(skinnedMesh);
                }
            }
            else
            {
                MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                for (var index = 0; index < meshFilters.Length; index++)
                {
                    combine[index].mesh = meshFilters[index].sharedMesh;
                    combine[index].transform = meshFilters[index].transform.localToWorldMatrix;
                    meshFilters[index].AddComponent<MeshCollider>();
                    
                    var renderer = meshFilters[index].GetComponent<MeshRenderer>();
                    renderer.enabled = true;
                    renderer.sharedMaterial = standardHeatMapMaterial;
                    _renderers.Add(renderer);
                }
            }
        }

        public void ClearHeatMap()
        {
            _currentMaterialPropertyBlock.Clear();

            foreach (var renderer in _renderers)
            {
                renderer.SetPropertyBlock(_currentMaterialPropertyBlock);
            }
        }

        public void SetHeatMap(float maxDistance, List<Vector4> points)
        {
            _currentMaterialPropertyBlock  = new MaterialPropertyBlock();

            foreach (var renderer in _renderers)
            {
                renderer.sharedMaterial = standardHeatMapMaterial;
                _currentMaterialPropertyBlock.SetInteger(PointSizeID, points.Count);
                _currentMaterialPropertyBlock.SetVectorArray(PointsID, points);
                _currentMaterialPropertyBlock.SetFloat(MaxDistanceID, maxDistance);
                renderer.SetPropertyBlock(_currentMaterialPropertyBlock);
            }
        }
    }
}