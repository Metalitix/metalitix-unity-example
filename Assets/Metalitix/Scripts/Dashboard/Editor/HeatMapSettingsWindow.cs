using System;
using Metalitix.Editor.Configs;
using Metalitix.Editor.Settings;
using Metalitix.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Editor
{
    public class HeatMapSettingsWindow : EditorWindow
    {
        private HeatMapSettings _heatMapSettings;
        private UnityEditor.Editor _heatMapEditor;

        private static HeatMapSettingsWindow _editor;

        private const int ViewerWindowWidth = 350;
        private const int ViewerWindowHeight = 120;

        public event Action OnApply;

        public void Initialize(Vector2 pos)
        {
            _editor = GetWindow<HeatMapSettingsWindow>();
            _editor.maxSize = new Vector2(ViewerWindowWidth, ViewerWindowHeight);
            _editor.minSize = _editor.maxSize;
            _editor.position = new Rect(pos.x, pos.y, position.width, position.height);
            _editor.titleContent = new GUIContent(EditorConfig.HeatMapSettings);
            _editor.Show();
        }

        private void OnEnable()
        {
            InitializeEditors();
            _heatMapSettings.SetHeatMapMaterial();
            _heatMapSettings.UpdateValues();
        }

        private void OnDisable()
        {
            AssetDatabase.SaveAssetIfDirty(_heatMapEditor);
        }

        private void InitializeEditors()
        {
            _heatMapSettings = MetalitixStartUpHandler.HeatMapSettings;
            _heatMapEditor = MetalitixEditorTools.CreateEditor(_heatMapSettings);
        }

        private void OnGUI()
        {
            MetalitixEditorTools.DrawInspector(_heatMapEditor);

            EditorGUILayout.BeginHorizontal();

            _heatMapSettings.CameraSize = EditorGUILayout.FloatField("Camera Size", _heatMapSettings.CameraSize);

            if (GUILayout.Button("Apply"))
            {
                if (EditorUtility.IsDirty(_heatMapSettings))
                {
                    OnApply?.Invoke();
                    AssetDatabase.SaveAssets();
                    EditorUtility.ClearDirty(this);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}