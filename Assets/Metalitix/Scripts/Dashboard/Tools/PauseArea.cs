using System;
using Metalitix.Core.Base;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Tools
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(RectTransform))]
    public class PauseArea : MonoBehaviour
    {
        private BoxCollider boxCollider;
        private RectTransform rectTransform;

        private PathPoint pausePoint;
        private PathPoint resumePoint;
        private bool isInitialized;

        private float pauseSliderValue;
        private float resumeSliderValue;

        public float PauseSliderValue => pauseSliderValue;
        public float ResumeSliderValue => resumeSliderValue;

        public PathPoint PausePoint => pausePoint;
        public PathPoint ResumePoint => resumePoint;
        

        public void Initialize(PathPoint pausePoint, PathPoint resumePoint)
        {
            this.pausePoint = pausePoint;
            this.resumePoint = resumePoint;
            boxCollider = GetComponent<BoxCollider>();
            rectTransform = GetComponent<RectTransform>();
            isInitialized = true;
        }

        public void SetWidth(float width)
        {
            CheckForInitialize();
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            boxCollider.size = new Vector3(width, rectTransform.sizeDelta.y * 2, 10);
        }

        public void SetPosX(float pos)
        {
            rectTransform.anchoredPosition = new Vector2(pos, 0);
        }

        public void SetPauseValue(float value)
        {
            CheckForInitialize();
            pauseSliderValue = value;
        }
        public void SetResumeValue(float value)
        {
            CheckForInitialize();
            resumeSliderValue = value;
        }

        private void CheckForInitialize()
        {
            if (!isInitialized)
            {
                throw new Exception("PauseArea is not initialized!");
            }
        }
    }
}
