using Metalitix.Scripts.Preview.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Metalitix.Scripts.Preview.Elements.Toggles
{
    public class PreviewIconToggle : PreviewToggleBase
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite onImage;
        [SerializeField] private Sprite offImage;

        public override void Initialize()
        {
            base.Initialize();
            OnToggleValueChanged += SetImage;
        }

        public void SetState(bool state)
        {
            SetImage(state);
            toggle.isOn = state;
        }

        private void SetImage(bool state)
        {
            image.sprite = state ? onImage : offImage;
        }
    }
}