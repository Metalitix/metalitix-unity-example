using Metalitix.Core.Base;
using UnityEngine;
using Metalitix.Scripts.Preview.Base;

namespace Metalitix.Scripts.Preview.Elements.Toggles
{
    public class PreviewTextToggle : PreviewToggleBase, IScenePreviewPointerMove
    {
        private Color _selectedColor;
        private Color _normalColor;

        public override void Initialize()
        {
            base.Initialize();
            _selectedColor = toggle.colors.selectedColor;
            _normalColor = toggle.colors.normalColor;

            toggle.onValueChanged.AddListener(ToggleValueChanged);

            if (toggle.isOn)
            {
                Interact();
            }
        }

        protected override void Update()
        {
            base.Update();
            toggle.targetGraphic.CrossFadeAlpha(toggle.isOn ? 1f : 0f, 0, true);
        }

        private void ToggleValueChanged(bool value)
        {
            toggle.targetGraphic.color = value ? _selectedColor : _normalColor;
        }

        public void PointerEnter()
        {
            if (toggle.isOn) return;

            toggle.targetGraphic.CrossFadeAlpha(1f, 0, true);
            toggle.targetGraphic.color = toggle.colors.highlightedColor;
        }

        public void PointerExit()
        {
            if (toggle.isOn) return;

            toggle.targetGraphic.CrossFadeAlpha(0f, 0.1f, true);
            toggle.targetGraphic.color = toggle.colors.normalColor;
        }
    }
}