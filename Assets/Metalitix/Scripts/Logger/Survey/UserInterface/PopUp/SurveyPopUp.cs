using System.Collections.Generic;
using Metalitix.Core.Enums;
using Metalitix.Core.Settings;
using Metalitix.Scripts.Logger.Survey.Base;
using Metalitix.Scripts.Logger.Survey.UserInterface.CustomButton;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Logger.Survey.UserInterface.PopUp
{
    [ExecuteInEditMode]
    public class SurveyPopUp : SurveyElement<RateType>
    {
        [SerializeField] private List<SurveyToggle> surveyToggles;
        [SerializeField] private Button submit;
        [SerializeField] private Image backGroundPanel;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private TMP_Text surveyText;
        [SerializeField] private RawImage logo;
        
        private SurveySettings _surveySettings;

        private const int TargetWidth = 128;
        private const int TargetHeight = 32;
        private const int MaxLogoHeight = 50;
        private const string SettingsPath = "Settings/SurveySettings";
        
        protected override void Start()
        {
            base.Start();
            
            _surveySettings = Resources.Load<SurveySettings>(SettingsPath);
            SyncWithSettings();
            
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

        public void SyncWithSettings()
        {
            if(_surveySettings == null) return;
            
            backGroundPanel.color = _surveySettings.SurveyBackgroundColor;
            buttonText.color = _surveySettings.SurveyButtonTextColor;

            ColorBlock colorBlock = submit.colors;
            colorBlock.normalColor = _surveySettings.SurveyButtonColor;
            submit.colors = colorBlock;
            
            surveyText.color = _surveySettings.SurveyTextColor;

            SetLogo(_surveySettings.SurveyLogo);
        }

        private void SetLogo(Texture2D logoTexture2D)
        {
            float logoAspectRatio = (float)logoTexture2D.width / logoTexture2D.height;
            float rawImageAspectRatio = (float)TargetWidth / TargetHeight;

            float scaleFactor = 1f;

            if (logoAspectRatio > rawImageAspectRatio)
            {
                scaleFactor = (float)TargetWidth / logoTexture2D.width;
            }
            else
            {
                scaleFactor = (float)TargetWidth / logoTexture2D.height;
            }

            scaleFactor = Mathf.Min(scaleFactor, (float)MaxLogoHeight / logoTexture2D.height);
            logo.texture = logoTexture2D;
            logo.rectTransform.sizeDelta = new Vector2(logoTexture2D.width * scaleFactor, logoTexture2D.height * scaleFactor);
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