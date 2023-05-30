using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public class HeatMapData : BackendData
    {
        public Record[] items { get; private set; }

        [JsonConstructor]
        public HeatMapData(Record[] items, Pagination pagination) : base(pagination)
        {
            this.items = items;
        }
    }
}