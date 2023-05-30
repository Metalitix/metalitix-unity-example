using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.PanelStates.SpeedPanelStates;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels
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