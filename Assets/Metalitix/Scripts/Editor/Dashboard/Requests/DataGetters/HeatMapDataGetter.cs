using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using UnityEditor;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataGetters
{
    public class HeatMapDataGetter : DataGetter<HeatMapData, Record>
    {
        public event Action OnBigDataReceived;
        
        public HeatMapDataGetter(DataRequest request, MetalitixBridge metalitixBridge) 
            : base(request, metalitixBridge)
        {
        }
        
        public override async Task<List<Record>> GetData()
        {
            do
            {
                data = await MetalitixBridge.GetHeatmap(ProjectData.id, AuthToken, PageQuery, Source.Token);

                if(data == null) break;
                
                if (data.pagination.totalItemsCount >= 5000)
                {
                    OnBigDataReceived?.Invoke();
                    break;
                }
                
                foreach (var item in data.items)
                {
                    LoadedData.Add(item);
                }

                await HandleLoading($"Records loading for project {ProjectData.title}", 
                    data.items.Length, data.pagination.totalItemsCount);
            } 
            while (LoadedCount < data.pagination.totalItemsCount && !Source.IsCancellationRequested);

            await Task.Delay(Delay);
            EditorUtility.ClearProgressBar();
            
            return LoadedData;
        }
    }
}