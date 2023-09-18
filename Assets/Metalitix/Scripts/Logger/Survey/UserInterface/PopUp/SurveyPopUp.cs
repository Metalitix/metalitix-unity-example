using System.Collections.Generic;
using Metalitix.Core.Enums;
using Metalitix.Scripts.Logger.Survey.Base;
using Metalitix.Scripts.Logger.Survey.UserInterface.CustomButton;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Logger.Survey.UserInterface.PopUp
{
    public class SurveyPopUp : SurveyElement<RateType>
    {
        [SerializeField] private List<SurveyToggle> surveyToggles;
        [SerializeField] private Button submit;

        protected override void Start()
        {
            base.Start();
            
            foreach (var surveyToggle in surveyToggles)
            {
                surveyToggle.OnButtonClicked += ActivateSubmitButton;
            }
            
            submit.onClick.AddListener(OnSubmitClick);
        }

        private void OnDisable()
        {
            foreach (var surveyToggle in surveyToggles)
            {
                surveyToggle.OnButtonClicked -= ActivateSubmitButton;
            }
            
            submit.onClick.RemoveListener(OnSubmitClick);
        }

        public void Activate()
        {
            submit.interactable = true;
        }
        
        private void ActivateSubmitButton(bool state, RateType rateType)
        {
            SetRate(state, rateType);
            submit.interactable = state;
        }
        
        private void OnSubmitClick()
        {
            if (!RateIsSet) return;
            
            ThrowEvent();
            Animate();
            submit.interactable = false;
        }
    }
}