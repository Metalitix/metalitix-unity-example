using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces
{
    [Serializable]
    public class SessionData
    {
        public string sessionId { get; private set; }
        public int projectId { get; private set; }
        public int duration { get; private set; }
        public string origin { get; private set; }
        public string userId { get; private set; }
        public DateTime startDate { get; private set; }
        public DateTime endDate { get; private set; }

        [JsonConstructor]
        public SessionData(string sessionId, int projectId, int duration, string origin, string userId, DateTime startDate, DateTime endDate)
        {
            this.sessionId = sessionId;
            this.projectId = projectId;
            this.duration = duration;
            this.origin = origin;
            this.userId = userId;
            this.startDate = startDate;
            this.endDate = endDate;
        }
    }
}