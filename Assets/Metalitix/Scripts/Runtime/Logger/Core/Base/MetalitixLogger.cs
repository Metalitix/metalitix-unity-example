using System;
using System.Linq;
using System.Threading.Tasks;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Containers;
using Metalitix.Scripts.Runtime.Logger.Core.Settings;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using Metalitix.Scripts.Runtime.Logger.Survey.Base;
using Metalitix.Scripts.Runtime.Logger.Survey.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Metalitix.Scripts.Runtime.Logger.Core.Base
{
    public class MetalitixLogger : MonoBehaviour
    {
        private bool _isInitialized;
        private SurveyElement<RateType> _currentPopUp;

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
        public MetalitixEventHandler EventHandler
        {
            get 
            {
                if (_metalitixSession == null)
                {
                    MetalitixDebug.Log(this,"Session has not started.", true);
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
                MetalitixDebug.Log(this, "Survey popup could not be found.", true);
        }

        private void OnDestroy()
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
                MetalitixDebug.Log(this,"No TrackingEntity found!", true);
            else
            {
                TrackingEntity = trackingEntity.First();
                MetalitixDebug.Log(this,$"TrackingEntity was found with name - {TrackingEntity.name}!");
            }
        }

        /// <summary>
        /// SetData logger, set userMeta and Camera data to the logger
        /// </summary>
        /// <param name="metalitixUserMetaData"></param>
        /// <param name="metalitixCameraData"></param>
        /// <param name="trackingEntity"></param>
        public virtual void Initialize(TrackingEntity trackingEntity = null)
        {
            if (trackingEntity != null)
                SetTrackingEntity(trackingEntity);
            else
                FindEntity();

            MetaData = new MetalitixUserMetaData(SceneManager.GetActiveScene().name);
            MetaData.SetSystemInfo(SystemInfoHelper.CollectSystemInfo());
            CameraData = new MetalitixCameraData(Camera.main);

            _fields = new MetalitixFields();

            _isInitialized = true;
            MetalitixDebug.Log(this,"Entity tracker is initialized!");
        }
        
        /// <summary>
        /// Call the start routine
        /// </summary>
        public void StartSession()
        {
            if (String.IsNullOrEmpty(GlobalSettings.APIKey))
            {
                MetalitixDebug.Log(this,"Please set API key in settings.", true);
                return;
            }
            
            if (CheckForInitialized())
            {
                if (_metalitixSession != null)
                {
                    if (_metalitixSession.IsPaused || _metalitixSession.IsRunning)
                    {
                        MetalitixDebug.Log(this, "Session has not ended yet!", true);
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
                    MetalitixDebug.Log(this, "Session has not started yet!", true);
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

        /// <summary>
        /// Set new custom field to MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
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
        public void SetCustomField(string fieldName, string value)
        {
            _fields?.AddField(fieldName, value);
            _recordsCreator?.SetFields(_fields);
        }

        /// <summary>
        /// Remove custom field from MetalitixTrackingData
        /// </summary>
        /// <param name="fieldName"></param>
        public void RemoveCustomField(string fieldName)
        {
            _fields.RemoveField(fieldName);
            _recordsCreator?.SetFields(_fields);
        }

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
            if (_currentPopUp == null)
            {
                MetalitixDebug.Log(this, "Please check if a Survey UI is added to the scene.", true);
                return;
            }
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
                MetalitixDebug.Log(this,"Logger has not initialized!");
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
            
            throw new Exception("Unable to load settings. Try re-importing the package.");
        }
    }
}