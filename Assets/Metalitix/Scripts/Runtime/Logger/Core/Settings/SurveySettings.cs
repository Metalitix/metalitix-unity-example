using Metalitix.Scripts.EditorAttributes;
using Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher;
using Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.PopUp;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Settings
{
    [CreateAssetMenu(fileName = "Metalitix/SurveySettings", menuName = "Metalitix/SurveySettings", order = 3)]
    public class SurveySettings : ScriptableObject
    {
        [MinMaxSlider(0.5f, 100)]
        [SerializeField] private Vector2 rangeOfTime = new (0.5f,5);
        [HideInInspector, SerializeField, Space(2f)] private ColorTheme currentColorTheme;
        [HideInInspector, SerializeField, Space(2f)] private SurveyPopUp surveyPopUp;
        [HideInInspector, SerializeField, Space(2f)] private bool wasInvoked;

        public ColorTheme CurrentTheme
        {
            get => currentColorTheme;
            set => currentColorTheme = value;
        }

        public SurveyPopUp SurveyPopUp
        {
            get => surveyPopUp;
            set => surveyPopUp = value;
        }

        public Vector2 RangeOfTimePopUpShowing => rangeOfTime;

        public bool WasInvoked
        {
            get => wasInvoked;
            set => wasInvoked = value;
        }
    }
}