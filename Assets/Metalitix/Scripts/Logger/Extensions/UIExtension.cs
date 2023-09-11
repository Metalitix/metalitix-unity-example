using UnityEngine;

namespace Metalitix.Scripts.Logger.Extensions
{
    public static class UIExtension
    {
        public static void ChangeVisible(this CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.alpha = state ? 1f : 0f;
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }
    }
}