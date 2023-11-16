using System.Collections.Generic;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Runtime;
using Metalitix.Scripts.Dashboard.Base;

namespace Metalitix.Scripts.Dashboard.Core.States.DashboardStates
{
    public class SessionState : DashboardState
    {
        private readonly List<PathPoint> _pathPoints;
        private readonly List<PathPoint> _eventPoints;
        private readonly SessionData _sessionData;

        public SessionState(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData, DashboardContainer dashboardContainer) : base(dashboardContainer)
        {
            _pathPoints = pathPoints;
            _eventPoints = eventPoints;
            _sessionData = sessionData;
        }

        public override void Enter()
        {
            var canvas = DashboardContainer.PreviewInteractableCanvas;
            var infoPanel = DashboardContainer.InfoPanel;
            var playerCamera = DashboardContainer.PlayerCamera;

            playerCamera.SetVisible(true);
            canvas.SetVisible(true);

            Subscribe();

            canvas.TimeLinePanel.SetTargetScene(DashboardContainer.TargetScene);
            infoPanel.SetSession(_pathPoints, _eventPoints, _sessionData);
            canvas.TimeLinePanel.SetSession(_pathPoints, _eventPoints, _sessionData);
        }

        public override void Exit()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            var canvas = DashboardContainer.PreviewInteractableCanvas;
            var infoPanel = DashboardContainer.InfoPanel;

            canvas.TimeLinePanel.OnChangeTimeLine += DashboardContainer.PlayerCamera.SetData;
            canvas.TimeLinePanel.OnEventClicked += infoPanel.Show;
            infoPanel.OnPointSelected += OnPointSelected;
        }


        private void Unsubscribe()
        {
            var canvas = DashboardContainer.PreviewInteractableCanvas;
            var infoPanel = DashboardContainer.InfoPanel;

            canvas.TimeLinePanel.OnChangeTimeLine -= DashboardContainer.PlayerCamera.SetData;
            canvas.TimeLinePanel.OnEventClicked -= infoPanel.Show;
            infoPanel.OnPointSelected -= OnPointSelected;
        }

        private void OnPointSelected(PathPoint point)
        {
            SetPlayerCameraPoint(point);
            DashboardContainer.PreviewInteractableCanvas.TimeLinePanel.SelectPoint(point);
        }

        private void SetPlayerCameraPoint(PathPoint point)
        {
            DashboardContainer.PlayerCamera.SetData(point.GetPosition, point.GetDirection, point.GetMetalitixCameraData);
        }
    }
}