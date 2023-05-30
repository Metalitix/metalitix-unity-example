using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Sliders
{
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
}