using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Extensions;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Metalitix.Scripts.Runtime.Dashboard.Algorithms
{
    public class PathDrawerAlgorithm : DrawerAlgorithm
    {
        private LineRenderer _lineRenderer;
        private PathPoint _pathPointPrefab;
        private PathPoint _eventPointPrefab;

        private readonly List<PathPoint> _eventPoints = new List<PathPoint>();
        private readonly List<PathPoint> _pathPoints = new List<PathPoint>();

        private const string Layer = "Preview";
        private const string PathPointKey = "PathPoint"; 
        private const string EventPointKey = "EventPoint"; 
        private const string ObjectKey = "Objects/";

        public event Action<List<PathPoint>, List<PathPoint>> OnPathReady; 

        public override async Task Initialize(float cameraSize, GameObject targetObject, List<Record> records, CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
            var go = new GameObject("LineRenderers");
            _lineRenderer = go.AddComponent<LineRenderer>();
            _lineRenderer.positionCount = 0;
            _lineRenderer.gameObject.layer = LayerMask.NameToLayer(Layer);
            _lineRenderer.widthMultiplier = 0.01f;
            _lineRenderer.numCornerVertices = 100;
            _pathPointPrefab = (PathPoint)Resources.Load(ObjectKey + PathPointKey, typeof(PathPoint));
            _eventPointPrefab = (PathPoint)Resources.Load(ObjectKey + EventPointKey, typeof(PathPoint));
            _lineRenderer.sharedMaterial = _pathPointPrefab.GetRenderer.sharedMaterial;
            SceneManager.MoveGameObjectToScene(go, Scene);

            Records = records;
        }

        public override async Task Draw()
        {
            await Task.Yield();
        }

        public override async Task DrawBySessionID(string id)
        {
            await DrawLine();
            await DrawPoints();
        }

        public override async Task Clear()
        {
            _lineRenderer.positionCount = 0;
            ClearList(_pathPoints);
            await Task.Yield();
        }

        private void ClearList(List<PathPoint> list)
        {
            foreach (PathPoint element in list)
            {
                Object.DestroyImmediate(element.gameObject);
            }

            list.Clear();
        }

        private async Task DrawLine()
        {
            var position = new List<Vector3>();
            
            foreach (var record in Records)
            {
                position.Add(record.data.position.MetalitixPosition());
            }

            _lineRenderer.positionCount = position.Count;
            _lineRenderer.SetPositions(position.ToArray());
            await Task.Yield();
        }

        private async Task DrawPoints()
        {
            int? eventID = null;
            
            for (var index = 0; index < Records.Count; index++)
            {
                var record = Records[index];
                if (CancellationToken.IsCancellationRequested)
                {
                    await Clear();
                    break;
                }
                
                PathPoint pathPoint;
                var pos = record.data.position.MetalitixPosition();
                var dir = record.data.direction.ConvertToUnityDirection();
                
                if (record.userEvent != null && !String.IsNullOrEmpty(record.userEvent.eventName))
                {
                    pathPoint = (PathPoint)PrefabUtility.InstantiatePrefab(_eventPointPrefab, Scene);
                    eventID ??= 0;
                    eventID++;
                    pathPoint.Initialize(index, record.timestamp, pos, dir, record.camera, eventID, record.userEvent.eventName);
                    _pathPoints.Add(pathPoint);
                    _eventPoints.Add(pathPoint);
                }
                else
                {
                    pathPoint = (PathPoint)PrefabUtility.InstantiatePrefab(_pathPointPrefab, Scene);
                    pathPoint.Initialize(index, record.timestamp, pos, dir, record.camera);
                    _pathPoints.Add(pathPoint);
                }
                
            }

            OnPathReady?.Invoke(_pathPoints, _eventPoints);
            await Task.Yield();
        }
    }
}