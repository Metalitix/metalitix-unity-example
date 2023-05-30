using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.GraphicsWorkers.ThemeSwitcher
{
    [CreateAssetMenu(fileName = "Metalitix/Theme", menuName = "Metalitix/Theme", order = 0)]
    public class ColorTheme : ScriptableObject
    {
        [SerializeField] private MetalitixThemeType themeType;
        [SerializeField] private Color mainColor;
        [SerializeField] private Color inverseColor;

        public Color MainColor => mainColor;
        public MetalitixThemeType ThemeType => themeType;
        public Color InverseColor => inverseColor;
    }
}