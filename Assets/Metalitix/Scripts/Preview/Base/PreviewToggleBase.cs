using System;
using Metalitix.Core.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Metalitix.Scripts.Preview.Base
{
    [RequireComponent(typeof(Toggle))]
    public abstract class PreviewToggleBase : PreviewInteractable, IScenePreviewInteractable
    {
        [SerializeField] protected Toggle toggle;

        public Toggle Toggle => toggle;

        public event Action<bool> OnToggleValueChanged;

        public virtual void Initialize()
        {
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
            }
        }

        public virtual void Interact()
        {
            if (!toggle.interactable) return;
#if UNITY_EDITOR
            ExecuteEvents.Execute(toggle.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
#endif
            OnToggleValueChanged?.Invoke(toggle.isOn);
        }
    }
}