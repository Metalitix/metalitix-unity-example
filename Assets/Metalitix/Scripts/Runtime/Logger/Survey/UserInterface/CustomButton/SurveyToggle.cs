using System;
using Metalitix.Scripts.Runtime.Logger.Survey.Enums;
using Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.Interfaces;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.CustomButton
{
    [RequireComponent(typeof(UnityEngine.UI.Toggle))]
    public class SurveyToggle : MonoBehaviour, IButtonValueHolder<RateType>
    {
        [SerializeField] private RateType type;
        
        private UnityEngine.UI.Toggle _toggle;
        public event Action<RateType> OnButtonClicked;
        
        private void Start()
        {
            _toggle = GetComponent<UnityEngine.UI.Toggle>();
            _toggle.onValueChanged.AddListener(OnClick);
        }

        private void OnClick(bool isOn)
        {
            if (isOn)
            {
                OnButtonClicked?.Invoke(type);
            }
        }
    }
}