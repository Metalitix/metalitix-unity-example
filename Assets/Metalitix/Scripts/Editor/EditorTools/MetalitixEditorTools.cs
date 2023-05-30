using System;
using Metalitix.Scripts.Editor.Tools.Styles;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.EditorTools
{
    public static class MetalitixEditorTools
    {
        public const float SpaceValueForLines = 1f;
        public const float SpaceValueForLogo = 25f;
        public const float SpaceValueBetweenButtons= 2f;
        public const float SpaceValueBeforeButtons= 20f;
        
        public const int EditorWindowWidth = 450;
        public const int EditorWindowHeight = 650;

        private static Color LineColor = Color.gray;

        private static void DrawUILine(Color color, int thickness = 1, int padding = 5)
        {
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y+= padding / 2;
            r.x-=2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        private static GUIStyle GetStyleForCenteredLinks()
        {
            var bodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 13,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
            };
            
            var linkStyle = new GUIStyle(bodyStyle)
            {
                wordWrap = false,
                normal =
                {
                    // Match selection color which works nicely for both light and dark skins
                    textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f)
                },
                stretchWidth = false
            };

            return linkStyle;
        }

        private static GUIStyle GetStyleForLinks()
        {
            var bodyStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                fontSize = 13,
                richText = true
            };
            
            var linkStyle = new GUIStyle(bodyStyle)
            {
                wordWrap = false,
                normal =
                {
                    // Match selection color which works nicely for both light and dark skins
                    textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f)
                },
                stretchWidth = false
            };

            return linkStyle;
        }

        public static GUIStyle GetButtonStyle()
        {
            return GenerateButtonStyle(13, TextAnchor.MiddleCenter, FontStyle.Bold);
        }

        public static GUIStyle GetSubHeaderTextStyle()
        {
            return GenerateTextStyle(20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        }
        
        public static GUIStyle GetHeaderTextStyle()
        {
            return GenerateTextStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        }
        
        public static GUIStyle GetSimpleTextStyle()
        {
            return GenerateTextStyle(12, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
        }

        private static GUIStyle GenerateTextStyle(int fontSize, FontStyle fontStyle, TextAnchor anchor, Color color)
        {
            var guiStyle = new GUIStyle
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                alignment = anchor,
                normal =
                {
                    textColor = color,
                    background = Texture2D.blackTexture
                }
            };

            return guiStyle;
        }

        private static GUIStyle GenerateButtonStyle(int fontSize, TextAnchor textAnchor, FontStyle fontStyle)
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
        
        public static bool LinkLabel(GUIContent label, bool useUnderLine, bool isCentered = false, params GUILayoutOption[] options)
        {
            GUIStyle style = null;
            
            style = isCentered ? GetStyleForCenteredLinks() : GetStyleForLinks();
            
            var position = GUILayoutUtility.GetRect(label, style, options);
            Handles.BeginGUI();
            Handles.color = style.normal.textColor;
            if(useUnderLine)
                Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, style);
        }

        public static UnityEditor.Editor CreateEditor(ScriptableObject scriptableObject)
        {
            return UnityEditor.Editor.CreateEditor(scriptableObject);
        }
        
        public static void DrawInspector(UnityEditor.Editor editor)
        {
            if (editor != null)
            {
                editor.OnInspectorGUI();

                if (editor.serializedObject.hasModifiedProperties)
                {
                    editor.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public static MetalitixStyle PaintCustomButton(MetalitixStyle button, Action onClicked = null)
        {
            if (GUI.Button(button.rect, button.name, button.style))
            {
                onClicked?.Invoke();
            }

            return button;
        }

        public static void PaintButton(string buttonName, Action onClicked = null)
        {
            EditorGUILayout.Space(SpaceValueBetweenButtons);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(buttonName, GetButtonStyle()))
            {
                onClicked?.Invoke();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(SpaceValueBetweenButtons);
        }
        
        public static void PaintSpaceWithLine()
        {
            EditorGUILayout.Space(SpaceValueForLines);
            DrawUILine(LineColor);
            EditorGUILayout.Space(SpaceValueForLines);
        }
    }
}