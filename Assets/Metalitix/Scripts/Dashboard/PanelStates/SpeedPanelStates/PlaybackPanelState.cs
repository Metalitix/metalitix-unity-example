using System;
using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Preview.Base;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.PanelStates.SpeedPanelStates
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