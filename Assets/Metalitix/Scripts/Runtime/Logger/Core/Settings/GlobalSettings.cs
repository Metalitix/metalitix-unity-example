using System;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Containers;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Settings
{
    [CreateAssetMenu(fileName = "Metalitix/GlobalSettings", menuName = "Metalitix/GlobalSettings", order = 4)]
    public class GlobalSettings : ScriptableObject
    {
        [SerializeField, Space(2f)] private bool useDebugMode;
        [SerializeField, Space(2f)] private bool useSurvey = false;
        [SerializeField, Range(0.2f, 5f)] private float inactivityInterval = 2f;
        [SerializeField, HideInInspector] private string apiKey = " ";
        [SerializeField, HideInInspector] private string serverUrl = MetalitixConfig.DevUrl;

        private string _tempApiKey;
        
        public string ServerUrl => serverUrl;

        public bool UseSurvey
        {
            get => useSurvey;
            set => useSurvey = value;
        }

        public bool UseDebugMode
        {
            get => useDebugMode;
            set => useDebugMode = value;
        }
        
        public string TempApiKey => _tempApiKey;

        public float InactivityInterval => inactivityInterval;

        public string APIKey
        {
            get => apiKey;
            set => apiKey = value.Trim();
        }

        [ContextMenu("Set Dev Server")]
        public void SetDevUrl()
        {
            serverUrl = MetalitixConfig.DevUrl;
        }
        
        [ContextMenu("Set Prod Server")]
        public void SetProdUrl()
        {
            serverUrl = MetalitixConfig.ProductionUrl;
        }
        
        [ContextMenu("Set Stage Server")]
        public void SetStageUrl()
        {
            serverUrl = MetalitixConfig.StageUrl;
        }

        public void SetTempApiKey(string tempApiKey = null)
        {
            _tempApiKey = String.IsNullOrEmpty(tempApiKey) ? apiKey : tempApiKey;
        }

        public void SetInactivityInterval(float value)
        {
            inactivityInterval = value;
        }
    }
}