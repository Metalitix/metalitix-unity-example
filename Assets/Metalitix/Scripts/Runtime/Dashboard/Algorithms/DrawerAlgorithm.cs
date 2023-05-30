using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Runtime.Dashboard.Visualizers;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metalitix.Scripts.Runtime.Dashboard.Algorithms
{
    public abstract class DrawerAlgorithm
    {
        protected CancellationToken CancellationToken;
        protected VisualWrapper Wrapper;
        protected GameObject TargetObject;
        protected Scene Scene;
        protected List<Record> Records;
        
        public void SetTargetScene(Scene targetScene)
        {
            Scene = targetScene;
        }
        
        public void SetWrapperObject(VisualWrapper newWrapper)
        {
            Wrapper = newWrapper;
        }

        public abstract Task Initialize(float cameraSize, GameObject targetObject, List<Record> points, CancellationToken cancellationToken);

        public abstract Task Clear();

        public abstract Task Draw();

        public virtual async Task PreloadDraw(List<Vector4> data)
        {
            await Task.Yield(); 
        }
        
        public virtual async Task DrawBySessionID(string id)
        {
            await Task.Yield();
        }

        protected void CheckCancelation()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                Clear();
                throw new TaskCanceledException();
            }
        }
    }
}