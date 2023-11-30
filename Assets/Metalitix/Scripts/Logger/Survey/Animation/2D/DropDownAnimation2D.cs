using System.Collections;
using Metalitix.Core.Enums;
using Metalitix.Scripts.Logger.Extensions;
using Metalitix.Scripts.Logger.Survey.Animation.Base;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Survey.Animation._2D
{
    public class DropDownAnimation2D : AnimationHolder
    {
        [SerializeField] private AnchorPreset anchorPreset;
        [Space(5f)] 
        [SerializeField] private Vector3 startPositionOnTheScreen;
        [SerializeField] private Vector3 startDestinationPosition;
        [Space(5f)]
        [SerializeField] private float timeOfTravel = 2f;

        private Vector3 _currentStartPoint;
        private Vector3 _currentDestinationPoint;
        private float _currentTime;
        private float _normalizedValue;
        
        protected override void Start()
        {
            base.Start();
            var anchor = AnchorExtension.GetAnchorPositions(anchorPreset);
            rectTransform.anchorMax = anchor;
            rectTransform.anchorMin = anchor;
            rectTransform.localPosition = startPositionOnTheScreen;
            _currentStartPoint = startPositionOnTheScreen;
            _currentDestinationPoint = startDestinationPosition;
        }

        public override IEnumerator Animate()
        {
            canvasGroup.ChangeVisible(true);
            
            _currentTime = 0f;
            
            while (_currentTime <= timeOfTravel)
            {
                _normalizedValue = _currentTime / timeOfTravel;
                rectTransform.anchoredPosition =Vector3.Lerp(_currentStartPoint,_currentDestinationPoint,  _normalizedValue);
                _currentTime += Time.deltaTime;
                yield return null; 
            }

            _currentDestinationPoint = _currentStartPoint;
            _currentStartPoint = rectTransform.anchoredPosition;
        }
    }
}