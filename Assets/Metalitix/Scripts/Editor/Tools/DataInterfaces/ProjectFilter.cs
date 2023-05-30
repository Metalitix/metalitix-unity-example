using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class ProjectFilter
    {
        public DateTime createdAt { get; }
        public DateTime updatedAt { get; }
        public string endDate { get; }
        public string startDate { get; }
        public int id { get; }
        public int projectId { get; }
        public int userId { get; }
        public string timeInterval { get; }
        public string name { get; }

        [JsonConstructor]
        public ProjectFilter(DateTime createdAt, string endDate, string startDate, DateTime updatedAt, int id, int projectId, int userId, string timeInterval, string name)
        {
            this.createdAt = createdAt;
            this.endDate = endDate;
            this.startDate = startDate;
            this.updatedAt = updatedAt;
            this.id = id;
            this.projectId = projectId;
            this.userId = userId;
            this.timeInterval = timeInterval;
            this.name = name;
        }
    }
}