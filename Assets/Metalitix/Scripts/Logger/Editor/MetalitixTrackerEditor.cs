using System;
using System.Globalization;
using System.Reflection;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Enums;
using Metalitix.Core.Settings;
using Metalitix.Editor.Configs;
using Metalitix.Editor.Data;
using Metalitix.Editor.Preview;
using Metalitix.Editor.Settings;
using Metalitix.Editor.Tools;
using Metalitix.Scripts.Dashboard.Editor;
using Metalitix.Scripts.Logger.Core.Base;
using Metalitix.Scripts.Logger.Survey.UserInterface.PopUp;
using Metalitix.Scripts.Preview.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Metalitix.Scripts.Logger.Editor
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
        private MetalitixCamera _metalitixCamera;
        private SurveyPopUp _currentPreviewPopUp;
        private SurveyPopUp _currentPopUp;
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
            _buildData = MetalitixStartUpHandler.BuildData;

            if (_surveySettings.SurveyLogo == null)
            {
                _surveySettings.SurveyLogo = MetalitixStartUpHandler.SurveyLogo;
            }

            _globalSettingsEditor = MetalitixEditorTools.CreateEditor(_globalSettings);
            _surveySettingsEditor = MetalitixEditorTools.CreateEditor(_surveySettings);
            _loggerSettingsEditor = MetalitixEditorTools.CreateEditor(_loggerSettings);
        }

        private void SetDefaultSurvey()
        {
            if (_surveySettings.SurveyPopUp == null)
            {
                _surveySettings.SurveyPopUp = Resources.Load<SurveyPopUp>("Objects/WhitePopUpSimpleCanvas");
            }
        }

        private void InitializePreviewScene()
        {
            var editorSettings = MetalitixStartUpHandler.EditorSettings;
            _previewRenderer = new ScenePreviewRenderer(100, -10, editorSettings.SurveyPreviewBackgroundColor, editorSettings.GraphicsFormatForScenePreview, position.width, position.height);
            
            var canvas = _previewRenderer.SpawnGameObject("Canvas", 
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvas.GetComponent<Canvas>();

            var survey = FindObjectOfType<SurveyPopUp>();

            if (survey != null)
            {
                _currentPopUp = survey;
                _surveySettings.SurveyPopUp = _currentPopUp;
                _previewRenderer.MoveToTheSceneWithInstance(_currentPopUp.gameObject);
            }
            else
            {
                _previewRenderer.InstantiatePrefab(_surveySettings.SurveyPopUp);
            }
            
            canvasComponent.renderMode = RenderMode.ScreenSpaceCamera;
            canvasComponent.worldCamera = _previewRenderer.RenderCamera;

            ActivateSurveyInThePreviewScene(canvas);
            SyncSurvey();
        }

        private void ActivateSurveyInThePreviewScene(GameObject canvas)
        {
            var objects = _previewRenderer.Scene.GetRootGameObjects();

            foreach (var gameObject in objects)
            {
                if (!gameObject.TryGetComponent<SurveyPopUp>(out var popUp)) continue;

                _currentPreviewPopUp = popUp;
                var popUpTransform = _currentPreviewPopUp.transform;
                popUpTransform.SetParent(canvas.transform);
                popUpTransform.localPosition = Vector3.zero;
                popUpTransform.localScale = Vector3.one * 5f;
                
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
                SetDefaultSurvey();
                var isSurveyNull = _currentPopUp == null;

                EditorGUILayout.LabelField("Engagement Survey", MetalitixEditorTools.GetSubHeaderTextStyle());
                MetalitixEditorTools.PaintSpaceWithLine();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (isSurveyNull)
                {
                    EditorGUILayout.HelpBox("Survey not found in the current scene. Please click the InitializeSurvey button",
                        MessageType.Warning, true);
                    MetalitixEditorTools.PaintSpaceWithLine();
                }
                
                _surveySettings.SurveyPopUp =
                    EditorGUILayout.ObjectField("Survey PopUp", _surveySettings.SurveyPopUp, typeof(Object),
                        false);

                EditorGUI.BeginDisabledGroup(_currentPopUp == null);
                EditorGUI.BeginChangeCheck();
                MetalitixEditorTools.DrawInspector(_surveySettingsEditor);
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

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBetweenButtons);
                MetalitixEditorTools.PaintButton("Initialize Survey", InitializePopUp);
                EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBetweenButtons);
                EditorGUILayout.EndVertical();
                MetalitixEditorTools.PaintSpaceWithLine();

                if (!isSurveyNull)
                {
                    SyncSurvey();
                    EditorGUILayout.LabelField("Preview", MetalitixEditorTools.GetSubHeaderTextStyle());
                    DrawPreviewScene();
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void SyncSurvey()
        {
            if(_currentPreviewPopUp != null)
            {
                _currentPreviewPopUp.SyncWithSettings();
            }
            
            if(_currentPopUp == null) return;
            
            EditorUtility.SetDirty(_currentPopUp);
            _currentPopUp.SyncWithSettings();
        }

        private void InitializePopUp()
        {
            var ratePopUp = FindObjectOfType<SurveyPopUp>();

            if (ratePopUp)
            {
                EditorUtility.DisplayDialog("Message", MetalitixEditorLogs.SurveyPopUpAlreadyExist, "Ok");
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
            EditorUtility.DisplayDialog("Success", MetalitixEditorLogs.SurveyPopUpInitialized, "Ok");
            _currentPopUp = FindObjectOfType<SurveyPopUp>();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        private void Login()
        {
            _authorizationWindow = CreateInstance<AuthorizationWindow>();
            _authorizationWindow.Initialize(new Vector2(position.x + position.width / 2, position.y + position.height / 2));
            _authorizationWindow.Show();
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