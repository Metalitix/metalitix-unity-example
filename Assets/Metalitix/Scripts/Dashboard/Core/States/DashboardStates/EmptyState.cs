using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Dashboard.Movement;

namespace Metalitix.Scripts.Dashboard.Core.States.DashboardStates
{
    public class EmptyState : DashboardState
    {
        public EmptyState(DashboardContainer dashboardContainer) : base(dashboardContainer)
        {
        }

        public override void Enter()
        {
            var playerCamera = DashboardContainer.PlayerCamera;
            var canvas = DashboardContainer.PreviewInteractableCanvas;
            var infoPanel = DashboardContainer.InfoPanel;
            var timeLine = DashboardContainer.PreviewInteractableCanvas.TimeLinePanel;

            DashboardContainer.DashboardCamera.SetStrategy(DashboardMovementType.Free);

            playerCamera.SetVisible(false);
            canvas.SetVisible(false);
            infoPanel.Close();
            timeLine.ResetPanel();
        }

        public override void Exit()
        {

        }
    }
}