using UnityEngine;

namespace Metalitix.Scripts.Editor.Tools.Styles
{
    public class MetalitixStyle
    {
        public Rect rect { get; }
        public string name { get; }
        public GUIStyle style { get; }

        public MetalitixStyle(Rect rect, string name, GUIStyle style = null)
        {
            style ??= GenerateButtonStyle(13, TextAnchor.MiddleCenter, FontStyle.Bold);
            
            this.style = style;
            this.rect = rect;
            this.name = name;
        }

        private GUIStyle GenerateButtonStyle(int fontSize, TextAnchor textAnchor, FontStyle fontStyle)
        {
            var guiStyle = new GUIStyle
            {
                fontSize = fontSize,
                alignment = textAnchor,
                fontStyle = fontStyle,
                normal = new GUIStyleState()
                {
                    textColor = Color.white,
                    background = Texture2D.grayTexture
                },
                active = new GUIStyleState()
                {
                    textColor = Color.gray,
                    background = Texture2D.whiteTexture
                }
            };

            return guiStyle;
        }
    }
}