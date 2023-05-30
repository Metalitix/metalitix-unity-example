using System.Collections;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Survey.Animation.Base
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class AnimationHolder : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        protected RectTransform rectTransform;

        protected virtual void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetCanvasGroup(CanvasGroup newCanvasGroup)
        {
            canvasGroup = newCanvasGroup;
        }

        public abstract IEnumerator Animate();
    }
}