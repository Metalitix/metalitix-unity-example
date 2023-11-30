using UnityEngine;

namespace Metalitix.Scripts.Preview.Tools
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasGroup))]
    public class VisibleUIElement : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        public virtual void SetVisible(bool state)
        {
            canvasGroup.interactable = state;
            canvasGroup.alpha = state ? 1 : 0;
            canvasGroup.blocksRaycasts = state;
        }

        public virtual void ChangeVisible()
        {
            var state = canvasGroup.interactable;

            canvasGroup.interactable = !state;
            canvasGroup.alpha = !state ? 1 : 0;
            canvasGroup.blocksRaycasts = !state;
        }
    }
}