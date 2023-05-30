using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(PreviewButton))]
    [RequireComponent(typeof(RectTransform))]
    public class EventButton : MonoBehaviour
    {
        private PathPoint _relativePoint;
        private PreviewButton _previewButton;
        private RectTransform _rectTransform;

        public PathPoint RelativePoint => _relativePoint;
        public RectTransform RectTransform => _rectTransform;

        public event Action<PathPoint> OnEventClicked;
        
        public void Initialize(PathPoint point)
        {
            _previewButton = GetComponent<PreviewButton>();
            _rectTransform = GetComponent<RectTransform>();
            _relativePoint = point;
            
            _previewButton.Button.onClick.AddListener(EventClick);
        }

        private void EventClick()
        {
            OnEventClicked?.Invoke(_relativePoint);
        }
    }
}