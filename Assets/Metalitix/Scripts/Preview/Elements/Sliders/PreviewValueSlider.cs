using Metalitix.Scripts.Preview.Base;
using Metalitix.Scripts.Preview.Tools;
using UnityEngine;

#if UNITY_EDITOR
using Metalitix.Editor.Data;
#endif

namespace Metalitix.Scripts.Preview.Elements.Sliders
{
#if UNITY_EDITOR

    public class PreviewValueSlider : PreviewSliderBase
    {
        [SerializeField] private SliderValuesContainer valuesContainer;

        public override void Interact(InteractableData interactableData)
        {
            var value = GetInteractedSliderValue(interactableData);
            slider.value = valuesContainer.GetValidSliderValue(value);
            base.Interact(interactableData);
        }

        public void SetDefault()
        {
            slider.value = valuesContainer.DefaultValue;
        }
    }
#endif
}