using System.Globalization;
using Metalitix.Scripts.EditorAttributes;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            string valueStr;
 
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    valueStr = prop.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    valueStr = prop.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    valueStr = prop.floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case SerializedPropertyType.String:
                    valueStr = prop.stringValue;
                    break;
                default:
                    valueStr = "(not supported)";
                    break;
            }
 
            EditorGUI.LabelField(position,label.text, valueStr);
        }
    }
}