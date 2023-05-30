using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class PreviewHighlitable : MonoBehaviour, IScenePreviewPointerMove
    {
        public virtual void PointerEnter()
        {
            
        }

        public virtual void PointerExit()
        {
            
        }
    }
}