using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Highlitables
{
    public class PreviewHighlitableHandle : PreviewHighlitable
    {
        [SerializeField] private FloatText floatText;
        [SerializeField] private PreviewSliderBase previewSliderBase;
        
        public override void PointerEnter()
        {
            floatText.SetText(previewSliderBase.Value);
            floatText.SetVisible(true);
        }

        public override void PointerExit()
        {
            floatText.SetVisible(false);
        }
    }
}