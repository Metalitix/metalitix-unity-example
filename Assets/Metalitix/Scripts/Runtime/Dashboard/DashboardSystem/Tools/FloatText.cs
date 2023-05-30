using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using TMPro;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools
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