using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class PreviewInteractableCanvas : PreviewInteractableUI
    {
        [SerializeField] private TimeLinePanel timeLinePanel;
        
        private Canvas _canvas;

        public TimeLinePanel TimeLinePanel => timeLinePanel;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = dashboardCamera.TargetRenderCamera;
            timeLinePanel.Initialize(DashboardCamera);
            SetVisible(false);
        }
    }
}