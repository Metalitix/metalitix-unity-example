using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Enums;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Core.Tools.RequestTools;
using Metalitix.Scripts.Logger.Extensions;
using Metalitix.Scripts.Logger.Survey.Base;
using Metalitix.Scripts.Logger.Survey.UserInterface.PopUp;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class MetalitixLogger : MonoBehaviour
    {
        private bool _isInitialized;
        private CancellationTokenSource _surveyCancellation;
        private SurveyElement<RateType> _currentPopUp;

        private InternetAccessChecker _internetAccessChecker;
        private MetalitixFields _fields;
        private RecordsCreator _recordsCreator;
        private MetalitixSession _metalitixSession;
        private MetalitixScene _metalitixScene;
        private RequestHeaders _customRequestHeaders;
        private WebRequestHelper _webRequestHelper;

        private string _ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";
        
        protected MetalitixUserMetaData MetaData { get; private set; }
        protected MetalitixCameraData CameraData { get; private set; }
        
        protected MetalitixCamera metalitixCamera { get; private set; }
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
            _metalitixScene = FindObjectOfType<MetalitixScene>();
        }

        private void FindPopUp()
        {
            _currentPopUp = FindObjectOfType<SurveyPopUp>();
            CheckForSurvey();
        }

        private async void OnDestroy()
        {
            if(_metalitixSession == null) return;
            
            _surveyCancellation.Cancel();
            await _metalitixSession.ForceDispose();
        }

        /// <summary>
        /// Find entity to track data
        /// </summary>
        protected void FindEntity()
        {
            var trackingEntity = FindObjectsOfType<MetalitixCamera>();

            if (trackingEntity.Length < 1)
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.PleaseAttachMetalitixCamera);
            else
            {
                metalitixCamera = trackingEntity.First();
                MetalitixDebug.Log(this,MetalitixRuntimeLogs.MetalitixCameraFound + metalitixCamera.name);
            }
        }

        /// <summary>
        /// SetData logger, set userMeta and Camera data to the logger
        /// </summary>
        /// <param name="metalitixCamera"></param>
        /// <param name="customIp"></param>
        public virtual async void Initialize(MetalitixCamera metalitixCamera, string customIp = null)
        {
            _internetAccessChecker = new InternetAccessChecker(GlobalSettings.ServerUrl);
            var isHasAccessToInternet = await _internetAccessChecker.CheckInternetAccess();
            
            if (!isHasAccessToInternet)
            {
                _isInitialized = false;
                return;
            } 
            
            SetTrackingEntity(metalitixCamera);
            
            if(!CheckForMetalitixCamera()) return;
                
            _webRequestHelper = new WebRequestHelper();
            MetaData = new MetalitixUserMetaData(SceneManager.GetActiveScene().name);
            MetaData.SetSystemInfo(SystemInfoHelper.CollectSystemInfo());
            CameraData = new MetalitixCameraData(Camera.main);
            _fields = new MetalitixFields();
            _isInitialized = true;
            MetalitixDebug.Log(this,MetalitixRuntimeLogs.LoggerIsInitialized);
            HandleCustomIp(customIp);

            if (this.metalitixCamera.AutomaticallyStartLogging && !IsRunning)
            {
                StartSession();
            }
        }

        /// <summary>
        /// Call the start routine
        /// </summary>
        public void StartSession()
        {
            if(!CheckForMetalitixCamera()) return;
            
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

                _surveyCancellation = new CancellationTokenSource();
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
            _recordsCreator = new RecordsCreator(_metalitixScene, MetaData, CameraData);
            _metalitixSession = new MetalitixSession(LoggerSettings, GlobalSettings, _recordsCreator, _webRequestHelper);

            _recordsCreator.SetTrackingEntity(metalitixCamera);
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
        
        private void HandleCustomIp(string customIp)
        {
            if(string.IsNullOrEmpty(customIp)) return;
            
            Regex ipv4Regex = new Regex(_ipv4Pattern);

            if (!ipv4Regex.IsMatch(customIp))
            {
                MetalitixDebug.LogError(this, MetalitixRuntimeLogs.IPHasNotBeenVerified);
                return;
            }
            
            _customRequestHeaders = new RequestHeaders();
            _customRequestHeaders.AddHeader(HeaderType.ForwardedFrom, customIp);
            _webRequestHelper.SetCustomRequestHeaders(_customRequestHeaders);
            MetalitixDebug.Log(this, MetalitixRuntimeLogs.IPAddressConfigured + customIp);
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
        public async Task EndSession()
        {
            if (_metalitixSession != null)
            {
                await _metalitixSession.EndSession();
                _surveyCancellation.Cancel();
                _metalitixSession.OnUserBecameAfk -= HandleBecameAfk;
                _metalitixSession = null;
            }
        }

        /// <summary>
        /// Set tracking entity
        /// </summary>
        /// <param name="metalitixCamera"></param>
        [Obsolete("Users should always specify the camera when they start the session. Changing the camera after starting a session can yield unexpected results. Set MetalitixCamera always using the `Initialize` method")]
        public void SetTrackingEntity(MetalitixCamera metalitixCamera)
        {
            if (metalitixCamera != null)
            {
                this.metalitixCamera = metalitixCamera;
                _recordsCreator?.SetTrackingEntity(this.metalitixCamera);
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
            _metalitixSession?.LogUserEvent("Event", @event);
        }
        
        public void LogState(string eventName, int value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent("State", @event);
        }
        
        public void LogState(string eventName, float value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent("State", @event);
        }
        
        public void LogState(string eventName, double value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent("State", @event);
        }
        
        public void LogState(string eventName, bool value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent("State", @event);
        }
        
        public void LogState(string eventName, string value)
        {
            var eventType = MetalitixUserEventType.SessionState;
            var @event = new MetalitixUserEvent(eventName, eventType);
            @event.AddField(eventName, value);
            _metalitixSession?.LogUserEvent("State", @event);
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
            
            await Task.Delay(TimeSpan.FromSeconds(randomValue * 60), _surveyCancellation.Token);
            
            if(!_surveyCancellation.IsCancellationRequested)
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

        private bool CheckForMetalitixCamera()
        {
            if (metalitixCamera == null)
            {
                MetalitixDebug.LogError(this,MetalitixRuntimeLogs.PleaseAttachMetalitixCamera);
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