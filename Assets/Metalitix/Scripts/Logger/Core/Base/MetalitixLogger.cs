using System;
using System.Linq;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Enums;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Scripts.Logger.Extensions;
using Metalitix.Scripts.Logger.Survey.Base;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class MetalitixLogger : MonoBehaviour
    {
        private bool _isInitialized;
        private SurveyElement<RateType> _currentPopUp;

        private InternetAccessChecker _internetAccessChecker;
        private MetalitixFields _fields;
        private RecordsCreator _recordsCreator;
        private MetalitixSession _metalitixSession;
        private MetalitixAnimation[] _metalitixAnimations;
        
        protected MetalitixUserMetaData MetaData { get; private set; }
        protected MetalitixCameraData CameraData { get; private set; }
        
        protected TrackingEntity TrackingEntity { get; private set; }
        protected GlobalSettings GlobalSettings { get; private set; }
        protected SurveySettings SurveySettings { get; private set; }
        protected LoggerSettings LoggerSettings { get; private set; }

        public event Action OnAwayFromKeyboard;
        
        /// <summary>
        /// Tells if the session running
        /// </summary>
        public bool IsRunning => _metalitixSession != null && _metalitixSession.IsRunning;

        /// <summary>
        /// Get event handler class with different methods for events sending
        /// </summary>
        [Obsolete("Instead, use methods directly from the logger, such as `LogEvent`, `LogState`")]
        public MetalitixEventHandler EventHandler
        {
            get 
            {
                if (_metalitixSession == null)
                {
                    MetalitixDebug.LogError(this,MetalitixRuntimeLogs.SessionHasNotStartedYet);
                    return null;
                }

                return _metalitixSession.MetalitixEventHandler;
            }
        }

        private void Awake()
        {
            LoggerSettings = LoadSettings<LoggerSettings>(MetalitixConfig.RecordSettingsPath);
            SurveySettings = LoadSettings<SurveySettings>(MetalitixConfig.RateSettingsKey);
            GlobalSettings = LoadSettings<GlobalSettings>(MetalitixConfig.GlobalSettingsPath);
            GlobalSettings.SetTempApiKey();
            
            MetalitixDebug.IsEnabled = GlobalSettings.UseDebugMode;

            DontDestroyOnLoad(gameObject);
            
            if(GlobalSettings.UseSurvey)
                FindPopUp();
        }

        private void Start()
        {
            _metalitixAnimations = FindObjectsOfType<MetalitixAnimation>();
        }

        private void FindPopUp()
        {
            _currentPopUp = FindObjectOfType<SurveyElement<RateType>>();

            if (_currentPopUp)
                _currentPopUp.SwitchTheme(SurveySettings.CurrentTheme);
            else
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.CheckSurveyInTheScene);
        }

        private void OnDisable()
        {
            EndSession();
        }

        /// <summary>
        /// Find entity to track data
        /// </summary>
        protected void FindEntity()
        {
            var trackingEntity = FindObjectsOfType<TrackingEntity>();

            if (trackingEntity.Length < 1)
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.PleaseAttachTrackingEntity);
            else
            {
                TrackingEntity = trackingEntity.First();
                MetalitixDebug.Log(this,MetalitixRuntimeLogs.TrackingEntityFound + TrackingEntity.name);
            }
        }

        /// <summary>
        /// SetData logger, set userMeta and Camera data to the logger
        /// </summary>
        /// <param name="trackingEntity"></param>
        public virtual async void Initialize(TrackingEntity trackingEntity = null)
        {
            _internetAccessChecker = new InternetAccessChecker();
            var isHasAccessToInternet = await _internetAccessChecker.CheckInternetAccess();

            if (!isHasAccessToInternet)
            {
                _isInitialized = false;
                return;
            } 
            
            if (trackingEntity != null)
                SetTrackingEntity(trackingEntity);
            else
                FindEntity();

            MetaData = new MetalitixUserMetaData(SceneManager.GetActiveScene().name);
            MetaData.SetSystemInfo(SystemInfoHelper.CollectSystemInfo());
            CameraData = new MetalitixCameraData(Camera.main);

            _fields = new MetalitixFields();

            _isInitialized = true;
            MetalitixDebug.Log(this,MetalitixRuntimeLogs.LoggerIsInitialized);

            if (TrackingEntity.AutomaticallyStartLogging && !IsRunning)
            {
                StartSession();
            }
        }
        
        /// <summary>
        /// Call the start routine
        /// </summary>
        public void StartSession()
        {
            if(!CheckForTrackingEntity()) return;
            
            if (String.IsNullOrEmpty(GlobalSettings.APIKey))
            {
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.PleaseSetAPIKey);
                return;
            }
            
            if (CheckForInitialized())
            {
                if (_metalitixSession != null)
                {
                    if (_metalitixSession.IsPaused || _metalitixSession.IsRunning)
                    {
                        MetalitixDebug.LogError(this, MetalitixRuntimeLogs.SessionHasNotEndedYet);
                        return;
                    }
                }

                CreateSession();

#if !UNITY_EDITOR
            if(!SurveySettings.WasInvoked)
                InvokePopUpWithDelay();
#endif
            
#if UNITY_EDITOR
                InvokePopUpWithDelay();
#endif             
            }
        }

        private void CreateSession()
        {
            _recordsCreator = new RecordsCreator(_metalitixAnimations, MetaData, CameraData);
            _metalitixSession = new MetalitixSession(LoggerSettings, GlobalSettings, _recordsCreator);

            _recordsCreator.SetTrackingEntity(TrackingEntity);
            _recordsCreator.SetFields(_fields);

            _metalitixSession.OnUserBecameAfk += HandleBecameAfk;
            
            _metalitixSession.StartSession(() =>
            {
                _metalitixSession = null;
                _recordsCreator = null;
            });

            if (_currentPopUp != null)
            {
                _currentPopUp.OnSurveyVoted += _metalitixSession.OnSurveyVoted;
            }
        }

        private async void HandleBecameAfk()
        {
            EndSession();
            OnAwayFromKeyboard?.Invoke();
            await _recordsCreator.WaitForUserBack();
            StartSession();
        }

        /// <summary>
        /// Manually sending new record to the server
        /// </summary>
        /// <param name="data"></param>
        public void UpdateSession()
        {
            _metalitixSession?.UpdateSession();
        }

        /// <summary>
        /// Change project APIKey
        /// </summary>
        /// <param name="apiKey"></param>
        public void ChangeAPIKey(string apiKey)
        {
            if (_metalitixSession != null)
            {
                EndSession();
                GlobalSettings.SetTempApiKey(apiKey);
                StartSession();
            }
            else
            {
                GlobalSettings.SetTempApiKey(apiKey);
            }
        }
        
        /// <summary>
        /// Create End record and stop writing process
        /// </summary>
        public void EndSession()
        {
            if (_metalitixSession != null)
            {
                if (!_metalitixSession.IsRunning && !_metalitixSession.IsPaused)
                {
                    MetalitixDebug.LogError(this, MetalitixRuntimeLogs.SessionHasNotStartedYet);
                    return;
                }
                
                _metalitixSession.EndSession();
                _metalitixSession.OnUserBecameAfk -= HandleBecameAfk;
                _metalitixSession = null;
            }
        }

        /// <summary>
        /// Set tracking entity
        /// </summary>
        /// <param name="trackingEntity"></param>
        public void SetTrackingEntity(TrackingEntity trackingEntity)
        {
            if (trackingEntity != null)
            {
                TrackingEntity = trackingEntity;
                _recordsCreator?.SetTrackingEntity(TrackingEntity);
            }
        }

        #region EventFunctions

        /// <summary>
        /// Sends the event 
        /// </summary>
        /// <param name="groupName">key</param>
        /// <param name="eventName">value</param>
        public void LogEvent(string groupName, string eventName)
        {
            var eventType = MetalitixUserEventType.custom;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.SetGroupName(groupName);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        public void LogState(string eventName, int value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        public void LogState(string eventName, float value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        public void LogState(string eventName, double value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        public void LogState(string eventName, bool value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        public void LogState(string eventName, string value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent(@event);
        }
        
        #endregion

        #region SettAttributeFunctions
        
        public void SetAttribute(string fieldName, int value)
        {
            SetCustomField(fieldName, value);
        }
        
        public void SetAttribute(string fieldName, float value)
        {
            SetCustomField(fieldName, value);
        }
        
        public void SetAttribute(string fieldName, double value)
        {
            SetCustomField(fieldName, value);
        }
        
        public void SetAttribute(string fieldName, bool value)
        {
            SetCustomField(fieldName, value);
        }
        
        public void SetAttribute(string fieldName, string value)
        {
            SetCustomField(fieldName, value);
        }

        public void RemoveAttribute(string fieldName)
        {
            RemoveCustomField(fieldName);
        }

        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Use the `SetAttribute` methods instead")]
        public void SetCustomField(string fieldName, int value) 
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }
        
        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Use the `SetAttribute` methods instead")]
        public void SetCustomField(string fieldName, bool value) 
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }
        
        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Use the `SetAttribute` methods instead")]
        public void SetCustomField(string fieldName, float value) 
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }
        
        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        [Obsolete("Use the `SetAttribute` methods instead")]
        public void SetCustomField(string fieldName, double value) 
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }
        
        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        [Obsolete("Use the `SetAttribute` methods instead")]
        public void SetCustomField(string fieldName, string value)
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }

        /// <summary>
        /// Remove custom field from MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        [Obsolete("Use the `RemoveAttribute` method instead")]
        public void RemoveCustomField(string fieldName)
        {
            _fields.RemoveField(fieldName);
            _recordsCreator?.SetFields(_fields);
        }
        
        #endregion

        /// <summary>
        /// Set poll interval to object tracker settings
        /// </summary>
        /// <param name="value"></param>
        public void SetPollInterval(float value)
        {
            _metalitixSession?.UpdatePollInterval(value);
        }

        /// <summary>
        /// Stop sending data without notify server
        /// </summary>
        public void PauseSession()
        {
            _metalitixSession?.PauseSession();
        }
        
        /// <summary>
        /// Start sending data without notify server
        /// </summary>
        public void ResumeSession()
        {
            _metalitixSession?.ResumeSession();
        }

        /// <summary>
        /// Show the survey popUp
        /// </summary>
        public void ShowSurveyPopUp()
        {
            if(!GlobalSettings.UseSurvey) return;
            if(_metalitixSession == null) return;
            if(!CheckForSurvey()) return;
            
            _currentPopUp.SetVisible(true);
            _currentPopUp.Animate();
            SurveySettings.WasInvoked = true;
        }
        

        private async Task InvokePopUpWithDelay()
        {
            var range = SurveySettings.RangeOfTimePopUpShowing;
            var randomValue = Random.Range(range.x, range.y);
            
            await Task.Delay(TimeSpan.FromSeconds(randomValue * 60));
            
            ShowSurveyPopUp();
        }

        private bool CheckForInitialized()
        {
            if (!_isInitialized)
            {
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.LoggerHasNotInitialized);
                return false;
            }

            return true;
        }

        private bool CheckForTrackingEntity()
        {
            if (TrackingEntity == null)
            {
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.PleaseAttachTrackingEntity);
                return false;
            }

            return true;
        }
        
        private bool CheckForSurvey()
        {
            if (_currentPopUp == null)
            {
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.CheckSurveyInTheScene);
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Load SO from Resources and set settings
        /// </summary>
        private static T LoadSettings<T>(string path) where T : ScriptableObject
        {
            var settings = Resources.Load<T>(path);

            if (settings != null)
                return settings;
            
            throw new Exception(MetalitixRuntimeLogs.UnableToLoadSettings);
        }
    }
}