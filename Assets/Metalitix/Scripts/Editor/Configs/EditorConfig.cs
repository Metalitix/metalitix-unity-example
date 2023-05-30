using UnityEngine;

namespace Metalitix.Scripts.Editor.Configs
{
    public static class EditorConfig
    {
        [Header("Links")]
        public const string MainLink = "https://app.metalitix.com/";
        public const string DocumentationLink = "https://docs.metalitix.com/Unity";
        
        [Header("Themes")]
        public const string LightThemeKey = "LightTheme";
        public const string DarkThemeKey = "DarkTheme";
        
        [Header("Settings")]
        public const string LoggerSettingsKey = "LoggerSettings";
        public const string SurveySettingsKey = "SurveySettings";
        public const string GlobalSettingsKey = "GlobalSettings";
        public const string DashboardSettings = "DashboardSettings";
        public const string HeatMapSettings = "HeatMapSettings";
        
        [Header("Graphics")]
        public const string LogoGraphic = "Logo";
        public const string SettingsGraphic = "Settings";
        
        [Header("Paths")]
        public const string BuildDataPath = "/Metalitix/BuildData.json";
        public const string LoggerTitle = "Logger";
        public const string DashboardTitle = "Dashboard";
        public const string ObjectKey = "Objects/";
        public const string SettingsKey = "Settings/";
        public const string GraphicsKey = "Graphics/";
        public const string AuthEditorSave = "AccessToken";
    }
}