using System;
using System.Text;
using System.Threading;
using Metalitix.Scripts.Editor.Configs;
using Metalitix.Scripts.Editor.Dashboard.Settings;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Enums;
using Metalitix.Scripts.Runtime.Logger.Core.Settings;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Tools.MetalitixTools
{
    [InitializeOnLoad]
    public static class MetalitixStartUpHandler
    {
        private static MetalitixBridge _metalitixBridge;
        
        [Header("Graphic")]
        private static Texture2D _logo;
        private static Texture2D _settings;
        
        [Header("Settings")]
        private static LoggerSettings _loggerSettings;
        private static SurveySettings _surveySettings;
        private static GlobalSettings _globalSettings;
        private static DashboardSettings _dashboardSettings;
        private static HeatMapSettings _heatMapSettings;
        private static BuildData _buildData;

        private static ColorTheme _mainTheme;
        private static ColorTheme _inverseTheme;

        public static BuildData BuildData => _buildData;
        public static Texture2D Logo => _logo;
        public static Texture2D Settings => _settings;
        public static DashboardSettings DashboardSettings => _dashboardSettings;
        public static LoggerSettings LoggerSettings => _loggerSettings;
        public static SurveySettings SurveySettings => _surveySettings;
        public static GlobalSettings GlobalSettings => _globalSettings;
        public static HeatMapSettings HeatMapSettings => _heatMapSettings;
        public static ColorTheme MainTheme => _mainTheme;
        public static ColorTheme InverseTheme => _inverseTheme;
        public static string GetApiKey => _globalSettings.APIKey;
        
        public const string PreviewInteractableLayer = "PreviewInteractable";
        public const string PreviewLayer = "Preview";
        
        static MetalitixStartUpHandler()
        {
            LoadScriptableObjects();
            LoadBuildData();
            LoadGraphics();
            Auth();
        }

        private static async void Auth()
        {
            _metalitixBridge = new MetalitixBridge(_globalSettings.ServerUrl);

            CheckLayers(new[] { PreviewInteractableLayer, PreviewLayer });
            
            if (!EditorPrefs.HasKey(EditorConfig.AuthEditorSave)) return;

            var token = EditorPrefs.GetString(EditorConfig.AuthEditorSave);
            _dashboardSettings.SetToken(token);
            
            if (_dashboardSettings.ValidateAuth())
            {
                var data = await _metalitixBridge.Me(_dashboardSettings.AuthToken, new CancellationToken());

                if (data != null)
                {
                    _dashboardSettings.SetAuthResponse(data);
                }
            }
        }
        
        private static void CheckLayers(string[] layerNames)
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = manager.FindProperty("layers");
 
            foreach (string name in layerNames)
            {
                // check if layer is present
                bool found = false;
                for (int i = 0; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                    
                    if (sp != null && name.Equals(sp.stringValue))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    SerializedProperty slot = null;
                    for (int i = 8; i <= 31; i++)
                    {
                        
                        SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                        if (sp != null && string.IsNullOrEmpty(sp.stringValue))
                        {
                            slot = sp;
                            break;
                        }
                    }
 
                    if (slot != null)
                    {
                        slot.stringValue = name;
                    }
                    else
                    {
                        Debug.LogError("Could not find an open Layer Slot for: " + name);
                    }
                }
            }
 
            // save
            manager.ApplyModifiedProperties();
        }
        
        private static void LoadGraphics()
        {
            _logo = (Texture2D)Resources.Load(EditorConfig.GraphicsKey + EditorConfig.LogoGraphic, typeof(Texture2D));
            _settings = (Texture2D)Resources.Load(EditorConfig.GraphicsKey + EditorConfig.SettingsGraphic, typeof(Texture2D));
        }

        private static void LoadBuildData()
        {
            _buildData = JsonHelper.FromJsonFile<BuildData>(
                Application.dataPath + EditorConfig.BuildDataPath, NullValueHandling.Ignore);

            var buildType = Enum.Parse<BuildType>(_buildData.buildType);

            switch (buildType)
            {
                case BuildType.Development:
                    _globalSettings.SetDevUrl();
                    break;
                case BuildType.Stage:
                    _globalSettings.SetStageUrl();
                    break;
                case BuildType.Production:
                    _globalSettings.SetProdUrl();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void LoadScriptableObjects()
        {
            _loggerSettings = TryLoadScriptable<LoggerSettings>(EditorConfig.LoggerSettingsKey);
            _surveySettings = TryLoadScriptable<SurveySettings>(EditorConfig.SurveySettingsKey);
            _globalSettings = TryLoadScriptable<GlobalSettings>(EditorConfig.GlobalSettingsKey);
            _dashboardSettings = TryLoadScriptable<DashboardSettings>(EditorConfig.DashboardSettings);
            _heatMapSettings = TryLoadScriptable<HeatMapSettings>(EditorConfig.HeatMapSettings);
            
            _mainTheme = TryLoadScriptable<ColorTheme>(EditorConfig.LightThemeKey);
            _inverseTheme = TryLoadScriptable<ColorTheme>(EditorConfig.DarkThemeKey);

            if (_loggerSettings != null && _surveySettings != null && _globalSettings != null &&
                _dashboardSettings != null && _heatMapSettings != null && _mainTheme != null && _inverseTheme != null) return;

            var str = new StringBuilder();
            str.AppendLine("One of the settings is missing!");
            str.AppendLine("Reimport plugin because it's seems to be corrupted");

            EditorUtility.DisplayDialog("Scriptable objects missing", str.ToString(), "Ok");

            throw new Exception("Scriptable objects missing");
        }
        
        private static T TryLoadScriptable<T>(string key) where T : ScriptableObject
        {
            var scriptable = Resources.Load<T>(EditorConfig.SettingsKey + key);

            if (scriptable == null)
                throw new Exception($"{typeof(T)} don`t founded");

            return scriptable;
        }
    }
}