using Metalitix.Scripts.Preview.Base;
using UnityEngine;

#if UNITY_EDITOR
using Metalitix.Editor.Data;
#endif

namespace Metalitix.Scripts.Preview.Elements.Sliders
{
#if UNITY_EDITOR
    [RequireComponent(typeof(RectTransform))]
    public class PreviewSlider : PreviewSliderBase
    {
        public override void Interact(InteractableData interactableData)
        {
            var value = GetInteractedSliderValue(interactableData);
            slider.value = value;
            base.Interact(interactableData);
        }
    }
#endif
}