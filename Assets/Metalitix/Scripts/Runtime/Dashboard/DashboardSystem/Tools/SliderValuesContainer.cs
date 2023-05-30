using System.Globalization;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools
{
    [ExecuteInEditMode]
    public class SliderValuesContainer : MonoBehaviour
    {
        [SerializeField] private float[] values;
        [SerializeField] private float defaultValue = 1f;
        
        public float[] GetValuesArray => values;
        public float DefaultValue => defaultValue;
        
        private void OnValidate()
        {
            values = new float[transform.childCount];

            for (var index = 0; index < transform.childCount; index++)
            {
                var obj = transform.GetChild(index);
                
                if (float.TryParse(obj.name, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    values[index] = value;
                }
            }
        }

        public float GetValidSliderValue(float value)
        {
            if (values.Length == 0) return 0;

            for (var index = 0; index < values.Length - 1; index++)
            {
                var arrayValue = values[index];
                var nextValue = values[index + 1];

                if (arrayValue < value && nextValue > value)
                {
                    float diffFirst = Mathf.Abs(value - arrayValue);
                    float diffSecond = Mathf.Abs(value - nextValue);
                    
                    return diffFirst < diffSecond ? arrayValue : nextValue;
                }
                
            }
            
            return 0;
        }
    }
}