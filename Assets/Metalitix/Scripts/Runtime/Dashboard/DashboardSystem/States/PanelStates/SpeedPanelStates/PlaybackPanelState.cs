using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.PanelStates.SpeedPanelStates
{
    public class PlaybackPanelState : PreviewInteractableUI
    {
        [SerializeField] private PreviewButton change;

        public event Action OnStateChanged;
        public event Action<float> OnValueChanged;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            change.Button.onClick.AddListener(InvokeStateChanged);
            base.Initialize(dashboardCamera);
        }

        protected void InvokeValueChanged(float value)
        {
            OnValueChanged?.Invoke(value);
        }
        
        private void InvokeStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }
}