using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Preview.Elements.Sliders;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.PanelStates.SpeedPanelStates
{
    public class SliderState : PlaybackPanelState
    {
#if UNITY_EDITOR
        [SerializeField] private PreviewValueSlider slider;
#endif
        public override void Initialize(DashboardCamera dashboardCamera)
        {
#if UNITY_EDITOR
            slider.Initialize(dashboardCamera.TargetRenderCamera);
            slider.OnSliderValueChanged += SendValue;
#endif
            base.Initialize(dashboardCamera);
        }

        public void SetDefault()
        {
#if UNITY_EDITOR
            slider.SetDefault();
#endif
        }

        private void SendValue(float value)
        {
            InvokeValueChanged(value);
        }
    }
}