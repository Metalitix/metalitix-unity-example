using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Dashboard.Settings
{
    [CreateAssetMenu(fileName = "Metalitix/HeatMapSettings", menuName = "Metalitix/HeatMapSettings", order = 0)]
    public class HeatMapSettings : ScriptableObject
    {
        [SerializeField, Range(0.1f, 10f)] 
        private float gazeIntensity = 0.1f;
        [SerializeField, Range(0.1f, 10f)]
        private float gazeScale = 0.1f;
        [SerializeField, Range(0.1f, 10f)]
        private float positionIntensity = 0.1f;
        [SerializeField, Range(0.1f, 10f)]
        private float positionScale = 0.1f;
        [SerializeField, HideInInspector] 
        private float cameraSize = 0.1f;
        
        [SerializeField, HideInInspector] 
        private Material heatMapMaterial;
        [SerializeField, HideInInspector] 
        private Material bodyMaterial;
        [SerializeField, HideInInspector] 
        private Material headMaterial;
        
        public float GazeIntensity => gazeIntensity;
        public float GazeScale => gazeScale;
        public float PositionIntensity => positionIntensity;
        public float PositionScale => positionScale;
        
        public float CameraSize 
        {
            get => cameraSize;
            set => cameraSize = value < 0.1f ? 0.1f : value;
        }

        private readonly int SpotScale = Shader.PropertyToID("_SpotScale");
        private readonly int SpotBrightness = Shader.PropertyToID("_SpotBrightness");
        private readonly int Scale = Shader.PropertyToID("_Scale");
        private readonly int Brightness = Shader.PropertyToID("_Brightness");
        
        private const string MaterialsPath = "Assets/Metalitix/Sources/Materials/";
        private const string HeatMapMaterial = "HeatMaterial.mat";
        private const string IndicatorBodyMaterial = "IndicatorBody.mat";
        private const string IndicatorHeadMaterial = "IndicatorHead.mat";
        
        public void SetHeatMapMaterial()
        {
            if(heatMapMaterial != null && bodyMaterial != null && headMaterial != null) return;
            
            heatMapMaterial = (Material)AssetDatabase.LoadAssetAtPath(MaterialsPath + HeatMapMaterial, typeof(Material));
            bodyMaterial = (Material)AssetDatabase.LoadAssetAtPath(MaterialsPath + IndicatorBodyMaterial, typeof(Material));
            headMaterial = (Material)AssetDatabase.LoadAssetAtPath(MaterialsPath + IndicatorHeadMaterial, typeof(Material));
        }

        public void SyncData(HeatMapSettingsData data)
        {
            gazeIntensity = data.heatmapBrightness;
            gazeScale = data.heatmapScaleFactor;
            positionIntensity = data.positionBrightness;
            positionScale = data.positionScaleFactor;
            cameraSize = data.cameraScaleFactor;
        }
        
        private void OnValidate()
        {
            EditorUtility.SetDirty(this);
            UpdateValues();
        }

        public void UpdateValues()
        {
            if(heatMapMaterial == null) return;
            
            heatMapMaterial.SetFloat(SpotScale, gazeScale);
            heatMapMaterial.SetFloat(SpotBrightness, gazeIntensity);
            bodyMaterial.SetFloat(Scale, positionScale);
            headMaterial.SetFloat(Scale, positionScale);
            bodyMaterial.SetFloat(Brightness, positionIntensity);
            headMaterial.SetFloat(Brightness, positionIntensity);
        }
    }
}