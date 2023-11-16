using UnityEngine;
using Metalitix.Scripts.Preview.Base;
using Metalitix.Scripts.Preview.Tools;

namespace Metalitix.Scripts.Preview.Elements.Highlitables
{
    public class PreviewHighlitableHandle : PreviewHighlitable
    {
        [SerializeField] private FloatText floatText;
#if UNITY_EDITOR
        [SerializeField] private PreviewSliderBase previewSliderBase;
#endif
        
        public override void PointerEnter()
        {
#if UNITY_EDITOR
            floatText.SetText(previewSliderBase.Value);
#endif
            floatText.SetVisible(true);
        }

        public override void PointerExit()
        {
            floatText.SetVisible(false);
        }
    }
}