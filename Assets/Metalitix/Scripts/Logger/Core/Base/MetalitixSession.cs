using System;
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
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    internal class MetalitixSession
    {
        private Batch _currentBatch;
        private DataSender<Record> _sender;
        private SessionTimer _sessionTimer;

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
                await TryToSend(data);
            }
            catch (Exception e)
            {
                onError?.Invoke();
                MetalitixDebug.LogError(this, e.Message);
            }

            _sessionTimer.LaunchTimer();
            CollectRecords();
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionHasStarted);
        }

        /// <summary>
        /// Create End record and stop writing process
        /// </summary>
        public async void EndSession()
        {
            if(!CheckIsRunning() && !IsPaused) return;
            
            var record = _recordsCreator.GetRecord(MetalitixEventType.SessionEnd, SessionUuid);

            await ManualSend(record);
            StopSession();

            _cancellationTokenSource.Dispose();
            MetalitixEventHandler = null;
            _sender = null;
            DeInitializeTimer();

            MetalitixDebug.Log(this, MetalitixRuntimeLogs.SessionHasEnded);
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
            _sessionTimer.StopTimer();
            _cancellationTokenSource.Cancel();
        }
        
        /// <summary>
        /// Creates an array of records and sends to the server if the array is full
        /// </summary>
        /// <returns></returns>
        private async Task CollectRecords()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            IsRunning = true;
            _currentBatch = new Batch();
            _currentBatch.OnDataPrepared += OnBatchDataPrepared;
            
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
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
            _currentBatch = new Batch();
            _currentBatch.OnDataPrepared += OnBatchDataPrepared;
            await Task.Run(() => _sender.SendData(data));
        }

        private async Task ManualSend(Record data)
        {
            var batch = new[] { data };
            await Task.Run(() => _sender.SendData(batch));
        }
        
        private async Task TryToSend(Record record)
        {
            for (var i = 1; i <= AttemptsToReconnect; i++)
            {
                try
                {
                    await ManualSend(record);
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