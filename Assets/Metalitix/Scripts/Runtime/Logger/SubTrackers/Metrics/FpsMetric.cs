using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Scripts.Runtime.Logger.SubTrackers.Data;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.SubTrackers.Metrics
{
    public class FpsMetric : Metric
    {
        private readonly List<int> _frames;
        private const int MaxFrames = 60;
        private FpsData _fpsData;
        
        private const float InitializeTime = 0.2f;
        
        public FpsMetric()
        {
            _fpsData = new FpsData();
            _frames = new List<int>(MaxFrames);
        }

        public override MetricData GetDataFromMetric()
        {
            return _fpsData;
        }

        public override async Task Initialize()
        {
            var timer = 0f;
            
            while (timer <= InitializeTime)
            {
                AddFrame();
                timer += Time.deltaTime;
                await Task.Yield();
            }

            Task.Yield();
        }
        
        public override async Task Proceed()
        {
            while (true)
            {
                AddFrame();
                await Task.Yield();
            }
        }

        private void AddFrame()
        {
            var currentFrame = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            _fpsData.SetCurrentFrame(currentFrame);
            _frames.Add(currentFrame);

            if (_frames.Count > MaxFrames)
                _frames.RemoveAt(0);
        }
 
        private void CalculateAverageFPS()
        {
            _fpsData.SetAverageFrame(0);
            
            var totalTimeOfAllFrames = 0;
            
            foreach (var frame in _frames)
                totalTimeOfAllFrames += frame;

            var averageFrame = totalTimeOfAllFrames / _frames.Count;
            _fpsData.SetAverageFrame(averageFrame) ;
        }
    }
}