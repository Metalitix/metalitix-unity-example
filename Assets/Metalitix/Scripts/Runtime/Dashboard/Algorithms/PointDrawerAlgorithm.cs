using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Extensions;
using Metalitix.Scripts.Runtime.Dashboard.Data;
using Metalitix.Scripts.Runtime.Dashboard.Visualizers;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Algorithms
{
    public class PointDrawerAlgorithm : DrawerAlgorithm
    {
        private float _maxPower = 1;
        private float _size = 1;
        private MetalitixPositionPoint _positionPoint;

        private const float ScaleOffset = 3;
        
        private readonly List<PointPower> _calculatedPoints = new List<PointPower>();
        private readonly List<MetalitixPositionPoint> _spawnedObjects = new List<MetalitixPositionPoint>();
        private readonly Dictionary<string, PointPower> _uniquePoints = new Dictionary<string, PointPower>();

        private const string PointKey = "Point"; 
        private const string ObjectKey = "Objects/";

        public override async Task Initialize(float cameraSize, GameObject targetObject, List<Record> points, CancellationToken cancellationToken)
        {
            _size = cameraSize;
            CancellationToken = cancellationToken;
            _positionPoint = (MetalitixPositionPoint)Resources.Load(ObjectKey + PointKey, typeof(MetalitixPositionPoint));
            Records = points;
            await Task.Yield();
        }

        public override async Task PreloadDraw(List<Vector4> preloadedHeatMap)
        {
            _maxPower = preloadedHeatMap.Max(v => v.w);
            
            foreach (var point in preloadedHeatMap)
            {
                var position = new Vector3(point.x, point.y, point.z).MetalitixPosition();
                var newPoint = new PointPower(position, point.w / _maxPower, 1);
                _calculatedPoints.Add(newPoint);
            }

            DrawPoints();

            await base.PreloadDraw(preloadedHeatMap);
        }
        
        public override async Task Draw()
        {
            CalculatePowers();
            DrawPoints();
        }

        private void CalculatePowers()
        {
            var tempPower = 0f;
            
            foreach (var record in Records)
            {
                if (record != null)
                {
                    var position = record.data.position.MetalitixPosition();
                    var dot = GetStep(position);
                    var slotKey = $"{dot.x}-{dot.y}-{dot.z}";

                    if (_uniquePoints.TryGetValue(slotKey, out var value))
                    {
                        tempPower = value.IncreasePower();
                        
                        if (_maxPower < tempPower)
                        {
                            _maxPower = tempPower;
                        }
                    }
                    else
                    {
                        var newPoint = new PointPower(dot, 1, 1);
                        _uniquePoints.Add(slotKey, newPoint);
                    }
                }
            }
            
            foreach (var uniquePair in _uniquePoints)
            {
                var uniquePower = uniquePair.Value.Power;
                var calculatedPower = uniquePower / _maxPower;
                uniquePair.Value.SetPower(calculatedPower);
                _calculatedPoints.Add(uniquePair.Value);
            }
        }

        private Vector3 GetStep(Vector3 position)
        {
            var newVector = new Vector3(
                Mathf.Ceil(position.x / _size) * _size - _size / 2,
                Mathf.Ceil(position.y / _size) * _size - _size / 2,
                Mathf.Ceil(position.z / _size) * _size - _size / 2);

            return newVector;
        }
        
        private void DrawPoints()
        {
            foreach (var point in _calculatedPoints)
            {
                CheckCancelation();
                MetalitixPositionPoint pointInstance = (MetalitixPositionPoint)PrefabUtility.InstantiatePrefab(_positionPoint, Scene);
                var transform = pointInstance.transform;
                var scale = transform.localScale;
                var calculatedValue = _size / ScaleOffset;
                
                transform.position = point.Position;
                transform.localScale = new Vector3(scale.x * calculatedValue, scale.y * calculatedValue, scale.z * calculatedValue);
                
                pointInstance.Visualize(point.Power);
                _spawnedObjects.Add(pointInstance);
            }
        }

        public override async Task Clear()
        {
            if(_spawnedObjects == null) return;
            
            foreach (var point in _spawnedObjects)
            {
                Object.DestroyImmediate(point.gameObject);
            }

            _spawnedObjects.Clear();
            await Task.Yield();
        }
    }
}