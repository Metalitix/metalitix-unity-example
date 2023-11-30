using System;
using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Dashboard.PanelStates.SpeedPanelStates;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Panels
{
    public class PlaybackPanel : PreviewInteractableUI
    {
        [SerializeField] private SliderState sliderState;

        public event Action<float> OnPlaybackSpeedChanged;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            SetVisible(false);
            sliderState.Initialize(dashboardCamera);
            InitializeSubscribers();
        }

        private void InitializeSubscribers()
        {
            sliderState.OnStateChanged += SetSliderState;
            sliderState.OnValueChanged += InvokePlaybackSpeedChanged;
        }

        public void SetDefault()
        {
            sliderState.SetDefault();
        }

        private void InvokePlaybackSpeedChanged(float value)
        {
            OnPlaybackSpeedChanged?.Invoke(value);
        }

        private void SetSliderState()
        {
            sliderState.SetVisible(false);
        }
    }
}