using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public class ProjectFilters : BackendData
    {
        public ProjectFilter[] items { get; }
        
        [JsonConstructor]
        public ProjectFilters(ProjectFilter[] items, Pagination pagination) : base(pagination)
        {
            this.items = items;
        }
    }
}