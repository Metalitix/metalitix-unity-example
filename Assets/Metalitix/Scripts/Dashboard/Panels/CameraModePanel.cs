using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Dashboard.Movement;
using Metalitix.Scripts.Preview.Elements.Toggles;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Panels
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
            if (!value) return;

            DashboardCamera.SetStrategy(DashboardMovementType.ThirdPerson);
        }

        private void FreeRoamMode(bool value)
        {
            if (!value) return;

            DashboardCamera.SetStrategy(DashboardMovementType.Free);
        }

        private void FirstPersonMode(bool value)
        {
            if (!value) return;

            DashboardCamera.SetStrategy(DashboardMovementType.FirstPerson);
        }
    }
}