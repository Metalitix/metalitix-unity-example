using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Enums;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Core.Tools.RequestTools;
using Metalitix.Scripts.Logger.Core.Base.Senders;
using Metalitix.Scripts.Logger.Core.Tools;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Logger.Core.Base
{
    internal class MetalitixSession
    {
        private Batch _currentBatch;
        private DataSender<Record> _sender;
        private SessionTimer _sessionTimer;
        private List<Task> _currentTasks;
        private bool _forceDisposed;
        private bool _isSessionStartSent;

        private CancellationTokenSource _cancellationTokenSource;

        private readonly LoggerSettings _loggerSettings;
        private readonly GlobalSettings _globalSettings;
        private readonly RecordsCreator _recordsCreator;
        private readonly WebRequestHelper _webRequestHelper;

        private const int AttemptsToReconnect = 20;
        private const float TimeToRestart = 1f;
        private const int MillisecondsInSecond = 1000;
        
        /// <summary>
        /// Tells if the system is running
        /// </summary>
        public bool IsRunning { get; private set; }
        
        /// <summary>
        /// Tells if the system was paused
        /// </summary>
        public bool IsPaused { get; private set; }
        
        public string SessionUuid { get; private set; }
        
        [Obsolete]
        public MetalitixEventHandler MetalitixEventHandler { get; private set; }

        public event Action OnUserBecameAfk;
        
        public MetalitixSession(LoggerSettings loggerSettings, GlobalSettings globalSettings, RecordsCreator recordsCreator, WebRequestHelper webRequestHelper)   
        {
            _loggerSettings = loggerSettings;
            _globalSettings = globalSettings;
            _recordsCreator = recordsCreator;
            _webRequestHelper = webRequestHelper;
            _currentTasks = new List<Task>();
        }
        
        public void UpdatePollInterval(float pollInterval)
        {
            _loggerSettings.CalculatePollInterval(pollInterval);
            MetalitixDebug.Log(this,MetalitixRuntimeLogs.PollIntervalHasChanged);
        }

        /// <summary>
        /// Create start record and start writing data to the server
        /// </summary>
        public async void StartSession(Action onError)
        {
            if (CheckIsRunning()) return;
            
            _sender = new KinesisSender(_globalSettings, _webRequestHelper);
            try
            {
                SessionUuid = await _sender.InitializeSession();
            }
            catch (Exception e) 
            {
                onError?.Invoke();
                return;
            }
            
            InitializeTimer();
            _recordsCreator.OnDataChanged += _sessionTimer.ResetAfkTime;

            var data = _recordsCreator.GetRecord(MetalitixEventType.SessionStart, SessionUuid);

            try
            {
                await TryToSendStartSession(data);

                if (_isSessionStartSent)
                {
                    MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionHasStarted);
                }
            }
            catch (Exception e)
            {
                onError?.Invoke();
                MetalitixDebug.LogError(this, e.Message);
            }

            _sessionTimer?.LaunchTimer();
            CollectRecords();
        }

        /// <summary>
        /// Create End record and stop writing process
        /// </summary>
        public async Task EndSession()
        {
            if (!CheckIsRunning() && !IsPaused)
            {
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.SessionHasNotStartedYet);
                return;
            }
            
            var record = _recordsCreator.GetRecord(MetalitixEventType.SessionEnd, SessionUuid);
            Dispose();
            await ManualSend(_currentBatch.GetArray());
            await ManualSend(record);
            _sender = null;
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionHasEnded);
            await Task.Yield();
        }

        public async Task ForceDispose()
        {
            await Task.WhenAll(_currentTasks);
            Dispose();
            
            if(_currentBatch != null && _currentBatch.items.Count != 0)
                await ManualSend(_currentBatch.GetArray());
            
            if (_isSessionStartSent)
            {
                var record = _recordsCreator.CurrentRecord.GetRecordWithNewType(MetalitixEventType.SessionEnd, DateTime.Now);

                if (record == null)
                {
                    MetalitixDebug.LogWarning(this, MetalitixRuntimeLogs.SessionEndedIncorrectly);
                }
                else
                {
                    await ManualSend(record);
                    MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionHasEnded);
                }
            }
            
            _sender = null;
            _forceDisposed = true;
            await Task.Yield();
        }
        
        private void Dispose()
        {
            StopSession();
            DeInitializeTimer();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Manually sending new record to the server
        /// </summary>
        /// <param name="data"></param>
        public void UpdateSession()
        {
            if (!IsRunning) return;
            
            var data = _recordsCreator.GetRecord(MetalitixEventType.SessionUpdate, SessionUuid);
            ManualSend(data);
            MetalitixDebug.Log(this,MetalitixRuntimeLogs.SessionUpdatedOccurred);
        }
        
        /// <summary>
        /// Stop sending data without notify server
        /// </summary>
        public void PauseSession()
        {
            if (!CheckIsRunning())
            {
                MetalitixDebug.Log(this, MetalitixRuntimeLogs.NotRunning);
                return;
            }
            
            var data = _recordsCreator.GetRecord(MetalitixEventType.SessionPause, SessionUuid);
            ManualSend(data);
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.Paused);
            StopSession();
        }
        
        /// <summary>
        /// Start sending data without notify server
        /// </summary>
        public void ResumeSession()
        {
            if (CheckIsRunning() && !IsPaused)
            {
                MetalitixDebug.Log(this, MetalitixRuntimeLogs.AlreadyRunning);
                return;
            }
            
            var data = _recordsCreator.GetRecord(MetalitixEventType.SessionResume, SessionUuid);
            ManualSend(data);
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.Resumed);
            ProceedSession();
        }
        
        public async void OnSurveyVoted(RateType rateType)
        {
            var data = new SurveysMetric(SessionUuid, _globalSettings.APIKey, (int)rateType);

            StringBuilder url = new StringBuilder();
            url.Append(_globalSettings.ServerUrl);
            url.Append(MetalitixConfig.SurveyEndPoint);

            string jsonData = JsonHelper.ToJson(data, NullValueHandling.Ignore);
            await _webRequestHelper.PostDataWithPlayLoad(url.ToString(), jsonData, new CancellationToken());
        }

        private void InitializeTimer()
        {
            _sessionTimer = new SessionTimer(_globalSettings.InactivityInterval * 60f);
            _sessionTimer.AfkTimeHasPassed += AfkTimePassed;
            _sessionTimer.OnSendTimeReached += OnSendTimeReached;
        }

        private void DeInitializeTimer()
        {
            if(_sessionTimer == null) return;
            
            _sessionTimer.AfkTimeHasPassed -= AfkTimePassed;
            _sessionTimer.OnSendTimeReached -= OnSendTimeReached;
            _sessionTimer.StopTimer();
            _sessionTimer = null;
        }

        private void AfkTimePassed()
        {
            OnUserBecameAfk?.Invoke();
        }
        
        private void ProceedSession()
        {
            IsRunning = true;
            IsPaused = false;
            _sessionTimer.ResumeTimer();
            CollectRecords();
        }
        
        private void StopSession()
        {
            IsRunning = false;
            IsPaused = true;
            _sessionTimer?.StopTimer();
            _cancellationTokenSource?.Cancel();
        }
        
        /// <summary>
        /// Creates an array of records and sends to the server if the array is full
        /// </summary>
        /// <returns></returns>
        private async Task CollectRecords()
        {
            if(_forceDisposed) return;
            
            _cancellationTokenSource = new CancellationTokenSource();
            IsRunning = true;
            _currentBatch = new Batch();
            _currentBatch.OnDataPrepared += OnBatchDataPrepared;
            
            while (!_cancellationTokenSource.Token.IsCancellationRequested && !_forceDisposed)
            {
                var record = _recordsCreator.GetRecord(MetalitixEventType.UserPosition, SessionUuid);
                if(record == null) break;
                
                _currentBatch.Add(record);
                await Task.Delay(TimeSpan.FromSeconds(_loggerSettings.PollInterval), _cancellationTokenSource.Token);
            }

            IsRunning = false;
        }

        private async void OnBatchDataPrepared(Record[] data)
        {
            _currentBatch.OnDataPrepared -= OnBatchDataPrepared;
            
            if(_forceDisposed) return;
            
            _currentBatch = new Batch();
            _currentBatch.OnDataPrepared += OnBatchDataPrepared;
            await Task.Run(() => _sender.SendData(data));
        }
        
        private async Task ManualSend(Record[] data)
        {
            var task = Task.Run(() => _sender.SendData(data));
            _currentTasks.Add(task);
            await task;
            _currentTasks.Remove(task);
        }

        private async Task ManualSend(Record data)
        {
            var batch = new[] { data };
            var task = Task.Run(() => _sender.SendData(batch));
            _currentTasks.Add(task);
            await task;
            _currentTasks.Remove(task);
        }
        
        private async Task TryToSendStartSession(Record record)
        {
            for (var i = 1; i <= AttemptsToReconnect; i++)
            {
                if(_forceDisposed) return;
                
                try
                {
                    await ManualSend(record);
                    _isSessionStartSent = true;    
                    return;
                }
                catch (Exception exception)
                {
                    var awaitingTime = (int)(MillisecondsInSecond * TimeToRestart);
                    MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionFailedToStartWithException + exception.Message + " " + MetalitixRuntimeLogs.RestartAttempt + TimeToRestart + "second");
                    await Task.Delay(awaitingTime);
                }
                
            }

            throw new Exception(MetalitixRuntimeLogs.SessionCouldNotStart);
        }
        
        private void OnSendTimeReached()
        {
            OnBatchDataPrepared(_currentBatch.GetArray());
        }

        /// <summary>
        /// Log an event from user
        /// </summary>
        /// <param name="metalitixUserEvent"></param>
        public void LogUserEvent(string type, MetalitixUserEvent metalitixUserEvent)
        {
            if (!IsRunning) return;
            
            var record = _recordsCreator.GetRecord(MetalitixEventType.UserInteraction, SessionUuid);
            record.SetUserEvent(metalitixUserEvent);
            ManualSend(record);
            
            MetalitixDebug.Log(this,$"{type} was logged: {metalitixUserEvent.eventName}");
        }
        
        private bool CheckIsRunning()
        {
            return IsRunning;
        }
    }
}