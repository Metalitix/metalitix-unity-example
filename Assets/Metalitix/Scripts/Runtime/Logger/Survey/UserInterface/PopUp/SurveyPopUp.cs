using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Survey.Base;
using Metalitix.Scripts.Runtime.Logger.Survey.Enums;
using Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.CustomButton;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.PopUp
{
    public class SurveyPopUp : SurveyElement<RateType>
    {
        [SerializeField] private List<SurveyToggle> surveyToggles;
        [SerializeField] private Button submit;

        protected override void Awake()
        {
            base.Awake();
            
            foreach (var surveyToggle in surveyToggles)
            {
                surveyToggle.OnButtonClicked += SetRate;
                surveyToggle.OnButtonClicked += (type) => ActivateSubmitButton();
            }
            
            submit.onClick.AddListener(OnSubmitClick);
        }

        private void ActivateSubmitButton()
        {
            submit.interactable = true;
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