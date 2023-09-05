using System;
using Metalitix.Core.Enums;
using Metalitix.Scripts.Logger.Survey.UserInterface.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Logger.Survey.UserInterface.CustomButton
{
    [RequireComponent(typeof(Toggle))]
    public class SurveyToggle : MonoBehaviour, IButtonValueHolder<RateType>
    {
        [SerializeField] private RateType type;
        
        private Toggle _toggle;
        public event Action<bool, RateType> OnButtonClicked;
        
        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(OnClick);
        }

        private void OnClick(bool isOn)
        {
            OnButtonClicked?.Invoke(isOn, type);
        }
    }
}