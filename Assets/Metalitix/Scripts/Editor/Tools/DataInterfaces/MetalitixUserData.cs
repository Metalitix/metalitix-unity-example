using System;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class MetalitixUserData
    {
        public string id { get; private set; }
        public string email { get; private set; }
        public string firstName { get; private set; }
        public string lastName { get; private set; }
        public string password { get; private set; }
        public string role { get; private set; }
        public string token { get; private set; }
        public string createdAt { get; private set; }
        public string updatedAt { get; private set; }

        public MetalitixUserData(string id, string email, string firstName, string lastName, string password, 
            string role, string token, string createdAt, string updatedAt)
        {
            this.id = id;
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.password = password;
            this.role = role;
            this.token = token;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
        }
    }
}