using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class MetalitixProjectsData : BackendData
    {
        public MetalitixProjectData[] items { get; private set; }

        [JsonConstructor]
        public MetalitixProjectsData(MetalitixProjectData[] items, Pagination pagination) : base(pagination)
        {
            this.items = items;
        }
    }
}