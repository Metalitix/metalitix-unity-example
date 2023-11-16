using Metalitix.Core.Base;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Metalitix.Scripts.Preview.Base
{
    [RequireComponent(typeof(Button))]
    public class PreviewButton : PreviewInteractable, IScenePreviewInteractable
    {
        [SerializeField] private Button button;

        public Button Button => button;

        public void Interact()
        {
            if (!button.interactable) return;
#if UNITY_EDITOR
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
#endif
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }
    }
}