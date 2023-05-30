using System.Linq;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public abstract class PreviewInteractable : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup[] parentCanvasGroups;
        
        private BoxCollider _boxCollider;

        protected virtual void Start()
        {
            _boxCollider = GetComponent<BoxCollider>();
        }

        protected virtual void Update()
        {
            if(parentCanvasGroups.Length == 0) return;

            var all = parentCanvasGroups.All(c => c.interactable);
            _boxCollider.enabled = all;
        }
    }
}