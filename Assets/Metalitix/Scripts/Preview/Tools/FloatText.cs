using TMPro;
using UnityEngine;

namespace Metalitix.Scripts.Preview.Tools
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasGroup))]
    public class FloatText : VisibleUIElement
    {
        [SerializeField] private TMP_Text currentText;

        public void SetText(float value)
        {
            currentText.text = $"{value}x";
        }
    }
}