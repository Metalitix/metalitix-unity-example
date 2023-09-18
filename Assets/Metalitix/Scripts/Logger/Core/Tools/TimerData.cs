using System;

namespace Metalitix.Scripts.Logger.Core.Tools
{
    public struct TimerData
    {
        private TimeSpan timePassed;
        
        public float elapsedTime { get; private set; }
        public float TimePassedInSeconds => timePassed.Seconds;

        public void AddElapsedTime(float elapsedTime)
        {
            this.elapsedTime += elapsedTime;
            timePassed = TimeSpan.FromSeconds(this.elapsedTime);
        }

        public void ResetValues()
        {
            timePassed = new TimeSpan();
            elapsedTime = 0;
        }
    }
}