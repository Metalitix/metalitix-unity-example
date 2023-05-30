using System;

namespace Metalitix.Scripts.Runtime.Logger.Survey.UserInterface.Interfaces
{
    public interface IButtonValueHolder<out T>
    {
        public event Action<T> OnButtonClicked;
    }
}