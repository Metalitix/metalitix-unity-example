using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Tools;
using Metalitix.Scripts.Logger.SubTrackers.Data;
using Metalitix.Scripts.Logger.SubTrackers.Metrics;

namespace Metalitix.Scripts.Logger.Core.Base
{
    internal class RecordsCreator
    {
        private FpsMetric _fpsMetric;
        private MetalitixFields _fields;
        private MetalitixCamera _metalitixCamera;
        private Record _currentRecord;

        private readonly MetalitixAnimation[] _metalitixAnimations;
        private readonly MetalitixUserMetaData _userMetaData;
        private readonly MetalitixCameraData _cameraData;
        private readonly List<Metric> _metrics = new List<Metric>();

        public event Action OnDataChanged;
        
        public RecordsCreator(MetalitixAnimation[] metalitixAnimations, MetalitixUserMetaData metaData, 
            MetalitixCameraData cameraData)
        {
            _userMetaData = metaData;
            _cameraData = cameraData;
            _metalitixAnimations= metalitixAnimations;
            
            InitilizeMetrics();
        }

        public void SetTrackingEntity(MetalitixCamera metalitixCamera)
        {
            _metalitixCamera = metalitixCamera;
        }
        
        public void SetFields(MetalitixFields fields)
        {
            _fields = fields;
        }

        /// <summary>
        /// Create record depend on type of metalitix event
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Record GetRecord(string type, string sessionUuid)
        {
            if (_metalitixCamera == null) return null;
            
            var data = new MetalitixTrackingData(_metalitixCamera.Position, _metalitixCamera.Direction);
            data.SetFields(_fields.GetFields());
            
            var record = new Record(type, sessionUuid, CurrentTimeStamp(), data);
            var animations = FetchAnimations();
            var metricData = GetMetricData();

            record.SetAnimations(animations);
            record.SetMetaData(_userMetaData);
            record.SetCameraData(_cameraData);
            record.SetMetrics(metricData);

            CheckForIdleState(record);
            _currentRecord = record;

            return record;
        }
        
        public async Task WaitForUserBack()
        {
            while (true)
            {
                if (_metalitixCamera != null)
                {
                    var data = new MetalitixTrackingData(_metalitixCamera.Position, _metalitixCamera.Direction);
                
                    if (!_currentRecord.data.Equals(data))
                    {
                        break;
                    }
                }
                
                await Task.Yield();
            }
        }

        private async void InitilizeMetrics()
        {
            _fpsMetric = new FpsMetric();
            _metrics.Add(_fpsMetric);
            
            foreach (var subTracker in _metrics)
            {
                await subTracker.Initialize();
                subTracker.Proceed();
                MetalitixDebug.Log(this, $"{subTracker.GetType().Name} initialized!");
            }
        }
        
        private void CheckForIdleState(Record record)
        {
            if (_currentRecord != null)
            {
                if (!_currentRecord.data.Equals(record.data) || _currentRecord.userEvent != null)
                {
                    OnDataChanged?.Invoke();
                }
            }
        }

        private AnimationData[] FetchAnimations()
        {
            if (_metalitixAnimations != null && _metalitixAnimations.Length > 0)
            {
                List<AnimationData> animationDatas = new List<AnimationData>();
                
                foreach (var animation in _metalitixAnimations)
                {
                    foreach (var data in animation.GetAnimationData())
                    {
                        animationDatas.Add(data);
                    }
                }

                return animationDatas.ToArray();
            }
            
            return null;
        }

        private MetricsData GetMetricData()
        {
            var metricData = new MetricsData();

            foreach (var metric in _metrics)
            {
                var dataFromMetric = metric.GetDataFromMetric();

                if (dataFromMetric is FpsData fpsData)
                    metricData.SetFps(fpsData.CurrentFrame);
            }

            return metricData;
        }
        
        /// <summary>
        /// Get current timestamp in milliseconds
        /// </summary>
        /// <returns></returns>
        private DateTime CurrentTimeStamp()
        {
            return DateTime.Now;
        }
    }
}