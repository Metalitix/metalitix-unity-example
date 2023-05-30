using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Extensions;
using Metalitix.Scripts.Intersections;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Algorithms
{
    public class HeatMapAlgorithm : DrawerAlgorithm
    {
        private Bounds _bounds;
        private float _minDistance = 0;
        private float _maxWeight = 0;

        private List<(Vector3, Vector3, Color)> _debugHits = new List<(Vector3, Vector3, Color)>();
        private List<Vector4> _debugSurface = new List<Vector4>();

        private List<Vector4> _mainSurface = new List<Vector4>();
        private List<Vector4> _nearestPoints = new List<Vector4>();
        private readonly List<Vector4> _surfaceHits = new List<Vector4>();

        public override async Task Initialize(float cameraSize, GameObject targetObject, List<Record> points, CancellationToken cancellationToken)
        {
            TargetObject = targetObject;
            Records = points;
            _bounds = BoundsExtension.GetBounds(TargetObject);
        }

        public override async Task PreloadDraw(List<Vector4> preloadedHeatMap)
        {
            List<Vector4> recalculatedHeatMap = new List<Vector4>();

            foreach (var heat in preloadedHeatMap)
            {
                var position = new Vector3(heat.x, heat.y, heat.z).MetalitixPosition();
                recalculatedHeatMap.Add(new Vector4(position.x, position.y, position.z, heat.w));
            }
            
            _mainSurface = recalculatedHeatMap;
            GetWeight(_mainSurface);
            Wrapper.SetHeatMap(_maxWeight, _mainSurface);
            await base.PreloadDraw(preloadedHeatMap);
        }
        
        public override async Task Draw()
        {
            await CalculateIntersects();
            var hitPoints = GetHitPoints(Records);
            CreateSurfaceMap(hitPoints);
            Wrapper.SetHeatMap(_maxWeight, _mainSurface);
            await Task.Yield();
        }

        public override async Task Clear()
        {
            if (Wrapper != null)
            {
                _mainSurface.Clear();
                _surfaceHits.Clear();
                Wrapper.ClearHeatMap();
                await Task.Yield();
            }
        }

        private async Task CalculateIntersects()
        {
            var physicsScene = Scene.GetPhysicsScene();
            var sideIntersection = new SideIntersection(_bounds, physicsScene);
            var faceIntersection = new FacesIntersection(_bounds, physicsScene);
            var verticalInterSection = new VerticalIntersection(_bounds, physicsScene);

            GetHits(sideIntersection);
            GetHits(faceIntersection);
            GetHits(verticalInterSection);

            await Task.Yield();
        }

        private void GetHits(IntersectionAlgorithm intersection)
        {
            foreach (var item in intersection.ViseVersaIntersect())
            {
                _surfaceHits.Add(item);
                _debugSurface.Add(item);
            }
        }

        private List<Vector3> GetHitPoints(List<Record> points)
        {
            var hitPoints = new List<Vector3>();
            var physicsScene = Scene.GetPhysicsScene();
            foreach (var point in points)
            {
                var convertedPos = point.data.position.MetalitixPosition();
                var convertedDirection = point.data.direction.ConvertToUnityDirection();
                var normalizedDirection = Quaternion.Euler(convertedDirection) * Vector3.forward;

                if (physicsScene.Raycast(convertedPos, normalizedDirection, out RaycastHit hit))
                {
                    _debugHits.Add((convertedPos, hit.point, Color.green));

                    hitPoints.Add(hit.point);
                }
            }
            return hitPoints;
        }

        private void CreateSurfaceMap(List<Vector3> heatMapHeats)
        {
            foreach (var heatMapHit in heatMapHeats)
            {
                var nearestPointIndex = 0;
                _minDistance = Mathf.Infinity;

                for (var index = 0; index < _surfaceHits.Count; index++)
                {
                    var hit = _surfaceHits[index];
                    var distance = Vector3.Distance(heatMapHit, hit);

                    if (_minDistance > distance)
                    {
                        nearestPointIndex = index;
                        _minDistance = distance;
                    }
                }

                var direction = heatMapHit - new Vector3(
                    _surfaceHits[nearestPointIndex].x, 
                    _surfaceHits[nearestPointIndex].y, 
                    _surfaceHits[nearestPointIndex].z);

                var scalarW = _surfaceHits[nearestPointIndex].w;
                var scalarValue = 1 - scalarW / (1 + scalarW);
                var scalarDirection = direction * scalarValue;

                var newPosition = new Vector3(
                    _surfaceHits[nearestPointIndex].x, 
                    _surfaceHits[nearestPointIndex].y, 
                    _surfaceHits[nearestPointIndex].z);

                var targetPosition = newPosition + scalarDirection;
                var increasedW = scalarW + 1;
                
                _surfaceHits[nearestPointIndex] = new Vector4(targetPosition.x, targetPosition.y, targetPosition.z, increasedW);
                _nearestPoints.Add(_surfaceHits[nearestPointIndex]);
            }

            if (_mainSurface.Count == 0)
            {
                _mainSurface = _surfaceHits;
            }

            //_mainSurface = MergeHeatMap(_mainSurface);
            GetWeight(_mainSurface);
        }
        
        private void GetWeight(List<Vector4> targetList)
        {
            _maxWeight = 0f;

            foreach (var value in targetList)
            {
                if (_maxWeight < value.w) 
                {
                    _maxWeight = value.w;
                }
            }
        }

        private List<Vector4> MergeHeatMap(List<Vector4> mainHeatMap)
        {
            for (var index = 0; index < mainHeatMap.Count; index++)
            {
                var main = mainHeatMap[index];
                var delta = _surfaceHits[index];
                
                var direction = delta - main;
                var scalarValue = main.w > 0 ? 1 - main.w / (delta.w + main.w) : 1;
                var scalarDirection = direction * scalarValue;
                var targetPosition = main + scalarDirection;
                var targetW = main.w + delta.w;
                
                mainHeatMap[index] = new Vector4(targetPosition.x, targetPosition.y, targetPosition.z, targetW);
            }

            return mainHeatMap;
        }
    }
}