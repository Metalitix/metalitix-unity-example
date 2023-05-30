using System;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class SurveysMetric
    {
        public string sessionId { get; }
        public string appkey { get; }
        public int rating { get; }

        public SurveysMetric(string sessionId, string appkey, int rating)
        {
            this.sessionId = sessionId;
            this.appkey = appkey;
            this.rating = rating;
        }
    }
}