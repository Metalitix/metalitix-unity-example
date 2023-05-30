using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class SessionsData : BackendData
    {
        public SessionData[] items { get; private set; }
        
        [JsonConstructor]
        public SessionsData(SessionData[] items, Pagination pagination) : base(pagination)
        {
            this.items = items;
        }
    }
}