using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Settings
{
    [CreateAssetMenu(fileName = "Metalitix/RecordSettings", menuName = "Metalitix/RecordSettings", order = 1)]
    public class LoggerSettings : ScriptableObject
    {
        [SerializeField, Range(0.1f, 2f)] private float pollInterval = 1f;

        private const float MAXPollInterval = 2f;
        private const float MINPollInterval = 0.1f;


        public float PollInterval => pollInterval;

        public void CalculatePollInterval(float value)
        {
            if(value >= 0.1f)
                pollInterval = Mathf.Min(MAXPollInterval, Mathf.Max(MINPollInterval, value));
        }
    }
}