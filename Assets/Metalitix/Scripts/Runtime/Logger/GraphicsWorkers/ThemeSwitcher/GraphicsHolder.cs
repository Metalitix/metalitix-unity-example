using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using Metalitix.Scripts.Runtime.Logger.Survey.Animation.Base;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class GraphicsHolder : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private List<AnimationHolder> animationHolders;
        [SerializeField] private List<GraphicsElement> graphics;
        [SerializeField] protected CanvasGroup canvasGroup;
        private ColorTheme _currentTheme;
        
        protected virtual void Awake()
        {
            foreach (var animationHolder in animationHolders)
            {
                animationHolder.SetCanvasGroup(canvasGroup);
            }
        }

        protected void ChangeVisible(bool state)
        {
            canvasGroup.ChangeVisible(state);
        }

        public virtual void Animate()
        {
            if (animationHolders.Count == 0)
            {
#if UNITY_EDITOR
                Debug.Log("Nothing to animate!");
#endif
                return;
            }
            
            foreach (var animationHolder in animationHolders)
            {
                StartCoroutine(animationHolder.Animate());
            }
        }
        
        public virtual void SwitchTheme(ColorTheme theme)
        {
            foreach (var graphic in graphics)
            {
                var color = graphic.Type switch
                {
                    GraphicType.Main => theme.MainColor,
                    GraphicType.Inverse => theme.InverseColor,
                    _ => Color.red
                };

                graphic.Graphic.color = color;
            }

            _currentTheme = theme;
        }
    }
}