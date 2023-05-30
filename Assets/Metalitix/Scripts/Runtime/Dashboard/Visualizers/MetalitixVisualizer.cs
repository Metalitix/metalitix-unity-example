using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Runtime.Dashboard.Algorithms;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metalitix.Scripts.Runtime.Dashboard.Visualizers
{
    [ExecuteInEditMode]
    public class MetalitixVisualizer : MonoBehaviour
    {
        [SerializeField] private VisualWrapper visualWrapper;

        private GameObject _targetGameObject;
        private Scene _currentScene;
        private float _cameraSize;
        private List<Record> _currentRecords;
        
        private readonly List<DrawerAlgorithm> _currentDrawers = new List<DrawerAlgorithm>();
        private readonly List<CancellationTokenSource> _cancelationSources = new List<CancellationTokenSource>();

        public Transform GetWrapperTransform()
        {
            return visualWrapper.transform;
        }
        
        public void VisualizeWrapper()
        {
            visualWrapper.Visualize();
        }

        public void Initialize(GameObject targetObject, Scene targetScene)
        {
            _targetGameObject = targetObject;
            _currentScene = targetScene;
        }
        
        public async Task SetData(float cameraSize, List<Record> records, CancellationTokenSource source, params DrawerAlgorithm[] drawersAlgorithms)
        {
            _currentRecords = records;
            _cameraSize = cameraSize;
            
            foreach (var drawerAlgorithm in drawersAlgorithms)
            {
                _currentDrawers.Add(drawerAlgorithm);
                drawerAlgorithm.SetWrapperObject(visualWrapper);
                drawerAlgorithm.SetTargetScene(_currentScene);
                await drawerAlgorithm.Initialize(_cameraSize, _targetGameObject, _currentRecords, source.Token);
            }

            _cancelationSources.Add(source);
        }
        
        public async Task Draw()
        {
            if(!ValidateRecords()) return;
            
            foreach (var drawer in _currentDrawers)
            {
                try
                {
                    await drawer.Draw();
                }
                catch (TaskCanceledException e)
                {
                    Debug.Log(e.Message);
                    throw;
                }
            }
        }

        public async Task DrawBySessionID(string id)
        {
            if(!ValidateRecords()) return;
            
            foreach (var drawer in _currentDrawers)
            {
                try
                {
                    await drawer.DrawBySessionID(id);
                }
                catch (TaskCanceledException e)
                {
                    Debug.Log(e.Message);
                    throw;
                }
            }
        }
        
        public async Task DeleteVisual()
        {
            if(_currentDrawers.Count == 0) return;

            StopTasks();
            
            foreach (var drawer in _currentDrawers)
            {
                await drawer.Clear();
            }
            
            _currentDrawers.Clear();
        }

        private void StopTasks()
        {
            foreach (var source in _cancelationSources)
            {
                CancelToken(source);
            }
            
            _cancelationSources.Clear();
        }

        private void CancelToken(CancellationTokenSource tokenSource) 
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }

        private bool ValidateRecords()
        {
            var value = _currentRecords == null || _currentRecords.Count == 0;
            return !value;
        }
    }
}