using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Sliders;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.PanelStates.SpeedPanelStates
{
    public class SliderState : PlaybackPanelState
    {
        [SerializeField] private PreviewValueSlider slider;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            slider.Initialize(dashboardCamera.TargetRenderCamera);
            slider.OnSliderValueChanged += SendValue;
            base.Initialize(dashboardCamera);
        }

        public void SetDefault()
        {
            slider.SetDefault();
        }

        private void SendValue(float value)
        {
            InvokeValueChanged(value);
        }
    }
}