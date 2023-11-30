using System;
using System.Collections.Generic;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Runtime;
using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Preview.Base;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Panels
{
    [ExecuteInEditMode]
    public class ToolsPanel : SessionPageInteractableUI
    {
        [SerializeField] private PreviewButton plus;
        [SerializeField] private PreviewButton minus;
        [SerializeField] private PreviewButton camera;
        [SerializeField] private PreviewButton texture;

        public event Action OnCameraModeClick;
        public event Action OnZoomDecrease;
        public event Action OnZoomIncrease;
        public event Action OnTextueClick;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            SetVisible(false);
        }

        public override void SetSession(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            SetVisible(true);
            InitializeSubscribers();
            base.SetSession(pathPoints, eventPoints, sessionData);
        }

        private void InitializeSubscribers()
        {
            plus.Button.onClick.AddListener(ZoomIncrease);
            minus.Button.onClick.AddListener(ZoomDecrease);
            camera.Button.onClick.AddListener(InvokeCameraMode);
            texture.Button.onClick.AddListener(TextureClick);
        }

        private void RemoveSubscribers()
        {
            plus.Button.onClick.RemoveListener(ZoomIncrease);
            minus.Button.onClick.RemoveListener(ZoomDecrease);
            camera.Button.onClick.RemoveListener(InvokeCameraMode);
            texture.Button.onClick.RemoveListener(TextureClick);
        }

        private void InvokeCameraMode()
        {
            OnCameraModeClick?.Invoke();
        }

        private void ZoomDecrease()
        {
            OnZoomDecrease?.Invoke();
        }

        private void ZoomIncrease()
        {
            OnZoomIncrease?.Invoke();
        }

        private void TextureClick()
        {
            OnTextueClick?.Invoke();
        }
    }
}