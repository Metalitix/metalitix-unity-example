using System;

namespace Metalitix.Scripts.Runtime.Logger.Survey.Base
{
    public interface ISurveyTracker<out T>
    {
        public event Action<T> OnSurveyVoted;

        public void SetVisible(bool state);
    }
}