using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.PanelStates.SpeedPanelStates
{
    public class CustomValueState : PlaybackPanelState
    {
        [SerializeField] private PreviewInputField valueInput;
        [SerializeField] private PreviewButton apply;
        
        public override void Initialize(DashboardCamera dashboardCamera)
        {
            apply.Button.onClick.AddListener(SendValue);
            base.Initialize(dashboardCamera);
        }

        private void SendValue()
        {
            var value = Convert.ToInt32(valueInput.Field.text); 
            InvokeValueChanged(value);
        }
    }
}