﻿using System;
using System.Globalization;
using Metalitix.Scripts.Editor.Configs;
using Metalitix.Scripts.Editor.Dashboard.EditorWindows;
using Metalitix.Scripts.Editor.Dashboard.Settings;
using Metalitix.Scripts.Editor.EditorTools;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Core.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using Metalitix.Scripts.Runtime.Logger.Core.Settings;
using Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher;
using Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.PopUp;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Metalitix.Scripts.Editor.Logger
{
    public class MetalitixTrackerEditor : EditorWindow
    {
        [Header("Main systems")]
        private MetalitixLogger _metalitixLogger;

        [Header("Settings")] 
        private GlobalSettings _globalSettings;
        private LoggerSettings _loggerSettings;
        private SurveySettings _surveySettings;
        private DashboardSettings _dashboardSettings;
        private ColorTheme _mainTheme;
        private ColorTheme _inverseTheme;
        private BuildData _buildData;

        [Header("Custom editors")]
        private UnityEditor.Editor _loggerSettingsEditor;
        private UnityEditor.Editor _surveySettingsEditor;
        private UnityEditor.Editor _globalSettingsEditor;

        private AuthorizationWindow _authorizationWindow;
        private TrackingEntity _trackingEntity;
        private SurveyPopUp _currentPreviewPopUp;
        private ScenePreviewRenderer _previewRenderer;
        
        private Vector2 _scrollPos;
        
        private static MetalitixTrackerEditor _editor;

        [MenuItem("Metalitix/Logger")]
        private static void ShowWindow()
        {
            _editor = GetWindow<MetalitixTrackerEditor>();
            _editor.maxSize = new Vector2(MetalitixEditorTools.EditorWindowWidth,
                MetalitixEditorTools.EditorWindowHeight);

            _editor.minSize = _editor.maxSize;
            _editor.titleContent = new GUIContent(EditorConfig.LoggerTitle);
            _editor.Show();
        }

        private void OnEnable()
        {
            InitializeSettings();
            InitializePreviewScene();
            
            EditorSceneManager.sceneClosing += EditorSceneManagerOnSceneClosing;
        }
        
        private void EditorSceneManagerOnSceneClosing(Scene scene, bool removingscene)
        {
            Close();
        }

        private void InitializeSettings()
        {
            _globalSettings = MetalitixStartUpHandler.GlobalSettings;
            _loggerSettings = MetalitixStartUpHandler.LoggerSettings;
            _surveySettings = MetalitixStartUpHandler.SurveySettings;
            _dashboardSettings = MetalitixStartUpHandler.DashboardSettings;
            _mainTheme = MetalitixStartUpHandler.MainTheme;
            _inverseTheme = MetalitixStartUpHandler.InverseTheme;
            _buildData = MetalitixStartUpHandler.BuildData;
            
            _globalSettingsEditor = MetalitixEditorTools.CreateEditor(_globalSettings);
            _surveySettingsEditor = MetalitixEditorTools.CreateEditor(_surveySettings);
            _loggerSettingsEditor = MetalitixEditorTools.CreateEditor(_loggerSettings);
            
            if (_surveySettings.CurrentTheme == null)
            {
                _surveySettings.CurrentTheme = _mainTheme;
            }
        }

        private void InitializePreviewScene()
        {
            _previewRenderer = new ScenePreviewRenderer(100, new Color(0.22f,0.22f,0.22f));
            var canvas = _previewRenderer.SpawnGameObject("Canvas", 
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent =  canvas.GetComponent<Canvas>();

            _previewRenderer.InstantiatePrefab(_surveySettings.SurveyPopUp);
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = _previewRenderer.RenderCamera;
        
            var objects = _previewRenderer.Scene.GetRootGameObjects();

            foreach (var gameObject in objects)
            {
                if (!gameObject.TryGetComponent<SurveyPopUp>(out var popUp)) continue;
                
                _currentPreviewPopUp = popUp;
                var popUpTransform = _currentPreviewPopUp.transform;
                popUpTransform.SetParent(canvas.transform);
                popUpTransform.localPosition = Vector3.zero;
                _currentPreviewPopUp.SwitchTheme(_surveySettings.CurrentTheme);
                _currentPreviewPopUp.SetVisible(true);
            }
        }

        private void OnDisable()
        {
            ClearInspector(_loggerSettingsEditor);
            ClearInspector(_surveySettingsEditor);
            ClearInspector(_globalSettingsEditor);
            
            DisposeCurrentPreviewScene();

            if (_currentPreviewPopUp != null)
            {
                DestroyImmediate(_currentPreviewPopUp);
            }
            
            AssetDatabase.SaveAssetIfDirty(_globalSettings);
            AssetDatabase.SaveAssetIfDirty(_surveySettings);
            AssetDatabase.SaveAssetIfDirty(_loggerSettings);
            
            EditorSceneManager.sceneClosing -= EditorSceneManagerOnSceneClosing; 
        }

        private void DisposeCurrentPreviewScene()
        {
            _previewRenderer.Dispose();
            _previewRenderer = null;
        }

        private void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _editor.Close();
            }
        }
        
        private void ClearInspector(Object target)
        {
            if (target == null)
            {
                return;
            }
 
            DestroyImmediate(target);
        }

        private void OnGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos,false,true,GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Version {_buildData.version.ToString(CultureInfo.InvariantCulture)} | {_buildData.buildType}");

            if (_dashboardSettings.ValidateAuth())
            {
                if (GUILayout.Button("Log Out"))
                {
                    _dashboardSettings.ClearResponse();
                }
            }
            else
            {
                if (GUILayout.Button("Log In"))
                {
                    Login();
                }
            }
            
            EditorGUILayout.EndHorizontal();

            if (_dashboardSettings.ValidateAuth())
            {
                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueForLines);
                EditorGUILayout.LabelField($"Welcome, {_dashboardSettings.UserFullName}", MetalitixEditorTools.GetSimpleTextStyle());
            }

            PaintLogo();
            DrawLinks();
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBeforeButtons);
            
            DrawGlobalSettings();
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBeforeButtons);
            PaintObjectTrackerSettings();
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBeforeButtons);
            DrawSurveySettings();
            GUILayout.EndScrollView();
        }

        private void DrawGlobalSettings()
        {
            EditorGUILayout.LabelField("Global Settings", MetalitixEditorTools.GetSubHeaderTextStyle());
            MetalitixEditorTools.PaintSpaceWithLine();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            MetalitixEditorTools.DrawInspector(_globalSettingsEditor);
            EditorGUILayout.LabelField("API Key", MetalitixEditorTools.GetSimpleTextStyle());
            _globalSettings.APIKey = EditorGUILayout.TextField("", _globalSettings.APIKey,
                new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter });
            EditorUtility.SetDirty(_globalSettings);
            EditorGUILayout.EndVertical();
        }

        private void DrawSurveySettings()
        {
            if (_globalSettings.UseSurvey)
            {
                EditorGUILayout.LabelField("Engagement Survey", MetalitixEditorTools.GetSubHeaderTextStyle());
                MetalitixEditorTools.PaintSpaceWithLine();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                MetalitixEditorTools.DrawInspector(_surveySettingsEditor);
                EditorGUI.BeginChangeCheck();
                _surveySettings.SurveyPopUp =
                    (SurveyPopUp)EditorGUILayout.ObjectField("Survey PopUp", _surveySettings.SurveyPopUp, typeof(SurveyPopUp), false);
                
                EditorUtility.SetDirty(_surveySettings);

                if (EditorGUI.EndChangeCheck())
                {
                    if (_surveySettings.SurveyPopUp != null)
                    {
                        DisposeCurrentPreviewScene();
                        InitializePreviewScene();
                    }
                    else
                    {
                        DisposeCurrentPreviewScene();
                    }
                }

                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBeforeButtons);
                MetalitixEditorTools.PaintButton("Switch Theme", SwitchTheme);
                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBetweenButtons);
                MetalitixEditorTools.PaintButton("Initialize PopUp", InitializePopUp);
                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBetweenButtons);
                EditorGUILayout.EndVertical();
                MetalitixEditorTools.PaintSpaceWithLine();
                
                if(_surveySettings.SurveyPopUp == null) return;
                
                EditorGUILayout.LabelField("Preview", MetalitixEditorTools.GetSubHeaderTextStyle());
                DrawPreviewScene();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void InitializePopUp()
        {
            var ratePopUp = FindObjectOfType<SurveyPopUp>();

            if (ratePopUp)
            {
                EditorUtility.DisplayDialog("Message", "Survey PopUp already exist", "Ok");
                return;
            }
            
            var canvas = FindObjectOfType<Canvas>();
            GameObject canvasInstance;
            
            if (canvas)
            {
                canvasInstance = canvas.gameObject;
            }
            else
            {
                canvasInstance = new GameObject
                {
                    name = "Canvas"
                };
                
                var canvasComponent = canvasInstance.AddComponent<Canvas>();
                var canvasScaler = canvasInstance.AddComponent<CanvasScaler>();
                canvasInstance.AddComponent<GraphicRaycaster>();

                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
            }
            
            PrefabUtility.InstantiatePrefab(_surveySettings.SurveyPopUp, canvasInstance.transform);
            EditorUtility.DisplayDialog("Success", "Survey PopUp successfully initialized", "Ok");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void Login()
        {
            _authorizationWindow = CreateInstance<AuthorizationWindow>();
            _authorizationWindow.Initialize(new Vector2(position.x + position.width / 2, position.y + position.height / 2));
            _authorizationWindow.Show();
        }

        private void SwitchTheme()
        {
            if (_currentPreviewPopUp != null)
            {
                switch (_surveySettings.CurrentTheme.ThemeType)
                {
                    case MetalitixThemeType.Light:
                        _currentPreviewPopUp.SwitchTheme(_inverseTheme);
                        _surveySettings.CurrentTheme = _inverseTheme;
                        break;
                    case MetalitixThemeType.Dark:
                        _currentPreviewPopUp.SwitchTheme(_mainTheme);
                        _surveySettings.CurrentTheme = _mainTheme;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DrawPreviewScene()
        {
            _previewRenderer.Render();
            var size = _previewRenderer.GetGUIPreviewSize();
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(size.y), GUILayout.ExpandHeight(false));
            EditorGUI.DrawPreviewTexture(rect, _previewRenderer.Texture2D);
        }

        private void DrawLinks()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (MetalitixEditorTools.LinkLabel(new GUIContent("Documentation"), true))
            {
                Application.OpenURL(EditorConfig.DocumentationLink);
            }
            
            GUILayout.Label("|");
            
            if (MetalitixEditorTools.LinkLabel(new GUIContent("Metalitix"),true))
            {
                Application.OpenURL(EditorConfig.MainLink);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        
        private void PaintLogo()
        {
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueForLogo);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(MetalitixStartUpHandler.Logo);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueForLogo);
        }

        private void PaintObjectTrackerSettings()
        {
            EditorGUILayout.LabelField("Logger Settings", MetalitixEditorTools.GetSubHeaderTextStyle());
            MetalitixEditorTools.PaintSpaceWithLine();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            MetalitixEditorTools.DrawInspector(_loggerSettingsEditor);
            EditorGUILayout.EndVertical();
            EditorUtility.SetDirty(_loggerSettings);
        }
    }
}