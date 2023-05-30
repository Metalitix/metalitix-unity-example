using System;

namespace Metalitix.Scripts.Extensions
{
    public class EnumValue<T> where T : Enum
    {
        private T _enumValue;
        private int _currentValue;

        public event Action<T> OnValueChanged;

        public int CurrentValue
        {
            get => _currentValue;
            set
            {
                if(value == _currentValue) return;

                _currentValue = value;
                _enumValue = (T)Enum.ToObject(typeof(T), value);
                OnValueChanged?.Invoke(_enumValue);
            }
        }
        
        public T CurrentEnumValue
        {
            get => (T)Enum.ToObject(typeof(T), _currentValue);
            set
            {
                _enumValue = value;
                _currentValue = (int)Enum.ToObject(typeof(T), _enumValue);
                OnValueChanged?.Invoke(_enumValue);
            }
        }

        public EnumValue(T enumValue)
        {
            _enumValue = enumValue;
            _currentValue = (int)(object)_enumValue;
        }

        public EnumValue(int value)
        {
            _currentValue = value;
            _enumValue = (T)Enum.ToObject(typeof(T), _currentValue);
        }
    }
}