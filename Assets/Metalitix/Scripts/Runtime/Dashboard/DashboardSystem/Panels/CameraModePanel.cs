using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Toggles;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels
{
    [ExecuteInEditMode]
    public class CameraModePanel : PreviewInteractableUI
    {
        [SerializeField] private PreviewTextToggle thirdPerson;
        [SerializeField] private PreviewTextToggle freeRoam;
        [SerializeField] private PreviewTextToggle firstPerson;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            SetVisible(false);
        }

        public void InitSession()
        {
            thirdPerson.OnToggleValueChanged += ThirdPersonMode;
            freeRoam.OnToggleValueChanged += FreeRoamMode;
            firstPerson.OnToggleValueChanged += FirstPersonMode;
            
            SetVisible(true);
            
            thirdPerson.Initialize();
            freeRoam.Initialize();
            firstPerson.Initialize();
        }
        
        private void ThirdPersonMode(bool value)
        {
            if(!value) return;
            
            DashboardCamera.SetStrategy(DashboardMovementType.ThirdPerson);
        }
        
        private void FreeRoamMode(bool value)
        {
            if(!value) return;
            
            DashboardCamera.SetStrategy(DashboardMovementType.Free);
        }
        
        private void FirstPersonMode(bool value)
        {
            if(!value) return;
            
            DashboardCamera.SetStrategy(DashboardMovementType.FirstPerson);
        }
    }
}