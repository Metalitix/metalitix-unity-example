using Metalitix.Scripts.Preview.Base;
using UnityEngine;

#if UNITY_EDITOR
using Metalitix.Editor.Data;
#endif

namespace Metalitix.Scripts.Preview.Elements.Sliders
{
#if UNITY_EDITOR
    public class PreviewSliderWithContainer : PreviewSliderBase
    {
        [SerializeField] private RectTransform container;
        [SerializeField] private float leftX;
        [SerializeField] private float rightX;

        public void SetTransformOnSlider(RectTransform rectTransform, float sliderValue)
        {
            rectTransform.SetParent(container, false);
            rectTransform.anchoredPosition = Vector3.zero;
            rectTransform.sizeDelta = new Vector2(30f, -20f);
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
            rectTransform.localScale = Vector3.one;

            float positionX = LerpPosBySliderValue(sliderValue);
            rectTransform.anchoredPosition = new Vector3(positionX, 0, 0);
        }

        public float CalculateWidthBetweenValues(float firstValue, float secondValue)
        {
            float positionFirst = LerpPosBySliderValue(firstValue);
            float positionSecond = LerpPosBySliderValue(secondValue);
            var delta = positionSecond - positionFirst;
            return delta;
        }

        public float GetPositionOnSlider(float sliderValue)
        {
            return LerpPosBySliderValue(sliderValue);
        }

        public override void Interact(InteractableData interactableData)
        {
            var value = GetInteractedSliderValue(interactableData);
            slider.value = value;
            base.Interact(interactableData);
        }

        private float LerpPosBySliderValue(float value)
        {
            return Mathf.Lerp(leftX, rightX, (value - slider.minValue) / (slider.maxValue - slider.minValue));
        }
    }
#endif
}