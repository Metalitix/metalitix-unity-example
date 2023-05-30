using System;
using System.Text;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base
{
    [RequireComponent(typeof(InputField))]
    public class PreviewInputField : PreviewInteractable, IScenePreviewInteractable
    {
        [SerializeField] private InputField inputField;

        private string _currentText;
        private bool _isFocused;
        private StringBuilder _stringBuilder;
        
        public InputField Field => inputField;

        public void Interact()
        {
            SetFocus();

            if (!_isFocused)
            {
                StopAllCoroutines();
                return;
            }
            
            if(!inputField.interactable) return;

            _stringBuilder = new StringBuilder();
        }

        protected override void Update()
        {
            base.Update();
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                _isFocused = false;
            }
            
            if (_isFocused) 
            {
                Debug.Log(Input.inputString);
                
                if(!String.IsNullOrEmpty(Input.inputString))
                {
                    var newValue = ValidateChar(Input.inputString);

                    if (!String.IsNullOrEmpty(newValue))
                    {
                        _stringBuilder.Append(newValue);
                        inputField.SetTextWithoutNotify(_stringBuilder.ToString());
                    }
                }
            }
            
            inputField.targetGraphic.color = _isFocused ? inputField.colors.selectedColor : inputField.colors.normalColor;
        }

        private string ValidateChar(string symbol)
        {
            if (int.TryParse(symbol, out var result))
            {
                return result.ToString();
            }
            
            if (symbol.Equals("."))
            {
                return symbol;
            }

            return null;
        }

        private void SetFocus()
        {
            _isFocused = !_isFocused;
        }
    }
}