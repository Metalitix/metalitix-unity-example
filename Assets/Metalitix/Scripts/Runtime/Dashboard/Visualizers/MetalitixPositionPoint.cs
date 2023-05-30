using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.Visualizers
{
    [ExecuteAlways]
    public class MetalitixPositionPoint : MonoBehaviour
    {
        [SerializeField] private MeshRenderer head;
        [SerializeField] private MeshRenderer body;

        private Gradient _heatGradient;

        private const float ColorBias = 0.1f;
        
        private readonly int ColorID = Shader.PropertyToID("_Color");
        private readonly int OpacityID = Shader.PropertyToID("_Opacity");
        private readonly int PowerID = Shader.PropertyToID("_Power");

        private void Awake()
        {
            InitializeGradient();
        }
        
        private void InitializeGradient()
        {
            _heatGradient = new Gradient();

            GradientColorKey[] colorKeys = new GradientColorKey[8];
            colorKeys[0].color = new Color32(117, 23, 20, 255);
            colorKeys[0].time = 0.0f;
            
            colorKeys[1].color = new Color32(150, 28, 30, 255);
            colorKeys[1].time = 0.142f;
            
            colorKeys[2].color = new Color32(213, 39, 48, 255);
            colorKeys[2].time = 0.284f;
            
            colorKeys[3].color = new Color32(255, 88, 47, 255);
            colorKeys[3].time = 0.426f;
            
            colorKeys[4].color = new Color32(255, 126, 0, 255);
            colorKeys[4].time = 0.568f;
            
            colorKeys[5].color = new Color32(255, 189, 0, 255);
            colorKeys[5].time = 0.710f;
            
            colorKeys[6].color = new Color32(255, 218, 72, 255);
            colorKeys[6].time = 0.858f;
            
            colorKeys[7].color = new Color32(255, 242, 184, 255);
            colorKeys[7].time = 1f;
   

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0].alpha = 0.3f;
            alphaKeys[0].time = 0.0f;
            alphaKeys[1].alpha = 0.3f;
            alphaKeys[1].time = 1.0f;
            
            _heatGradient.SetKeys(colorKeys, alphaKeys);
        }

        public void Visualize(float power)
        {
            var color = GetIndicatorColor(power);
            var calculatedOpacityValue = power + (1 - power) * 0.2f;

            var propertyBlock = new MaterialPropertyBlock();
            
            propertyBlock.SetVector(ColorID, new Vector4(color.r, color.g, color.b, 1));
            propertyBlock.SetFloat(PowerID, power);
            propertyBlock.SetFloat(OpacityID, calculatedOpacityValue);

            VisualizeHead(propertyBlock);
            VisualizeBody(propertyBlock);
        }

        private void VisualizeHead(MaterialPropertyBlock materialPropertyBlock)
        {
            head.SetPropertyBlock(materialPropertyBlock);
        }
        
        private void VisualizeBody(MaterialPropertyBlock materialPropertyBlock)
        {
            body.SetPropertyBlock(materialPropertyBlock);
        }
        
        private Color GetIndicatorColor(float time)
        {
            var targetColor = _heatGradient.Evaluate(time);
            return targetColor;
        }
    }
}