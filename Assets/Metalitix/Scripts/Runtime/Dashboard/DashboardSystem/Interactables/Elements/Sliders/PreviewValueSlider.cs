using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Sliders
{
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
}