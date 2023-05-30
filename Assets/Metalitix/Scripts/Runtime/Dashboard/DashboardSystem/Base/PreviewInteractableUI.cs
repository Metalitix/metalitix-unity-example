using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PreviewInteractableUI : MonoBehaviour
    {
        protected CanvasGroup CanvasGroup;
        protected DashboardCamera DashboardCamera;

        public virtual void Initialize(DashboardCamera dashboardCamera)
        {
            DashboardCamera = dashboardCamera;
            CanvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void SetVisible(bool state)
        {
            CanvasGroup.interactable = state;
            CanvasGroup.alpha = state ? 1 : 0;
            CanvasGroup.blocksRaycasts = state;
        }
        
        public virtual void ChangeVisible()
        {
            var state = CanvasGroup.interactable;
            
            CanvasGroup.interactable = !state;
            CanvasGroup.alpha = !state ? 1 : 0;
            CanvasGroup.blocksRaycasts = !state;
        }
    }
}