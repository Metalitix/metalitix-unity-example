using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base
{
    [ExecuteInEditMode]
    public abstract class PreviewObject<T, TV> : MonoBehaviour, IScenePreviewInteractable where T : Renderer where TV : Collider
    {
        [SerializeField] protected T renderer;
        [SerializeField] private TV collider;
        
        public T GetRenderer => renderer;

        public abstract void Interact();

        protected void Update()
        {
            collider.enabled = renderer.isVisible;
        }
    }
}