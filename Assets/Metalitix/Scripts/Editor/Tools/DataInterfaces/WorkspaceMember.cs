using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class WorkspaceMember
    {
        public int workspaceId { get; private set; }
        public string userId { get; private set; }
        public string firstName { get; private set; }
        public string lastName { get; private set; }
        public string fullName { get; private set; }
        public string role { get; private set; }
        public string status { get; private set; }

        [JsonConstructor]
        public WorkspaceMember(int workspaceId, string userId, string firstName, string lastName, string fullName,
            string role, string status)
        {
            this.workspaceId = workspaceId;
            this.userId = userId;
            this.firstName = firstName;
            this.lastName = lastName;
            this.fullName = fullName;
            this.role = role;
            this.status = status;
        }
    }
}