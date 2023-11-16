using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Core.Data.Runtime;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;
using UnityEditor;

namespace Metalitix.Scripts.Dashboard.Editor.Requests.DataGetters
{
#if UNITY_EDITOR
    public class SessionDataGetter : DataGetter<HeatMapData, Record>
    {
        private readonly string _sessionId;

        public SessionDataGetter(string sessionId, DataRequest request, MetalitixBridge metalitixBridge)
            : base(request, metalitixBridge)
        {
            _sessionId = sessionId;
        }

        public override async Task<List<Record>> GetData()
        {
            do
            {
                data = await MetalitixBridge.GetSessionHeatMapData(ProjectData.id, _sessionId, AuthToken, PageQuery, Source.Token);

                if (data == null) break;

                foreach (var item in data.items)
                {
                    LoadedData.Add(item);
                }

                await HandleLoading("Records loading for session", data.items.Length,
                    data.pagination.totalItemsCount);
            }
            while (LoadedCount < data.pagination.totalItemsCount && !Source.IsCancellationRequested);

            await Task.Delay(Delay);
            EditorUtility.ClearProgressBar();

            return LoadedData;
        }
    }
#endif
}