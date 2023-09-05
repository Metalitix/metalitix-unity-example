using System;

namespace Metalitix.Scripts.Logger.Survey.UserInterface.Interfaces
{
    public interface IButtonValueHolder<out T>
    {
        public event Action<bool, T> OnButtonClicked;
    }
}