using System;

namespace Metalitix.Scripts.Logger.Survey.Base
{
    public interface ISurveyTracker<out T>
    {
        public event Action<T> OnSurveyVoted;

        public void SetVisible(bool state);
    }
}