using System;
using Metalitix.Scripts.Logger.GraphicsWorkers.ThemeSwitcher;

namespace Metalitix.Scripts.Logger.Survey.Base
{
    public class SurveyElement<T> : GraphicsHolder, ISurveyTracker<T>
    {
        private T _currentRate;
        
        public event Action<T> OnSurveyVoted;
        public event Action OnClose;

        public T CurrentRate => _currentRate;
        public bool RateIsSet { get; private set; }

        public void SetVisible(bool state)
        {
            ChangeVisible(state);
        }

        protected void Close()
        {
            ChangeVisible(false);
            OnClose?.Invoke();
            RateIsSet = false;
        }

        protected void SetRate(bool state, T rate)
        {
            _currentRate = rate;
            RateIsSet = state;
        }

        protected void ThrowEvent()
        {
            OnSurveyVoted?.Invoke(_currentRate);
        }
    }
}