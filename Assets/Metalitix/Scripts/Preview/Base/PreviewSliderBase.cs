using System;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using Metalitix.Editor.Preview;
using Metalitix.Editor.Data;
#endif

namespace Metalitix.Scripts.Preview.Base
{
#if UNITY_EDITOR
    [RequireComponent(typeof(RectTransform))]
    public abstract class PreviewSliderBase : PreviewInteractable, IScenePreviewInteractableData
    {
        [SerializeField] protected Slider slider;
        [SerializeField] protected CanvasScaler canvasScaler;

        protected Camera Camera;
        protected RectTransform CanvasRect;
        protected RectTransform RectTransform;

        public event Action<float> OnSliderValueChanged;

        public float Value => slider.value;

        public void Initialize(Camera cam)
        {
            RectTransform = GetComponent<RectTransform>();
            Camera = cam;
            CanvasRect = canvasScaler.GetComponent<RectTransform>();
        }

        public void SetMaxValue()
        {
            slider.value = slider.maxValue;
        }

        public void SetMinValue()
        {
            slider.value = slider.minValue;
        }

        public void SetValue(float value)
        {
            slider.value = value;
        }

        public virtual void Interact(InteractableData interactableData)
        {
            OnSliderValueChanged?.Invoke(slider.value);
        }

        protected float GetInteractedSliderValue(InteractableData interactableData)
        {
            var rectOfCanvas = CanvasRect.rect;
            var sliderWidth = RectTransform.rect.width / canvasScaler.scaleFactor;
            var aspectRatio = interactableData.width / rectOfCanvas.width;
            var aspectRationPosition = rectOfCanvas.width / 1920f;
            var screePos = Camera.WorldToScreenPoint(RectTransform.position) * aspectRationPosition;
            var leftX = (screePos.x - sliderWidth / 2) * aspectRatio;
            var rightX = (screePos.x + sliderWidth / 2) * aspectRatio;
            var normalizedLeft = leftX / interactableData.width;
            var normalizedRight = rightX / interactableData.width;

            if (interactableData.uv.x < normalizedLeft || interactableData.uv.x > normalizedRight) return slider.value;

            float normal = Mathf.InverseLerp(normalizedLeft, normalizedRight, interactableData.uv.x);
            float value = Mathf.Lerp(slider.minValue, slider.maxValue, normal);
            var roundedValue = (float)Math.Round((decimal)value, 2, MidpointRounding.AwayFromZero);
            return roundedValue;
        }
    }
#endif
}