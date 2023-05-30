using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataGetters
{
    public abstract class DataGetter<T, TV> : RequestCreator where T : BackendData where TV : class
    {
        protected readonly MetalitixProjectData ProjectData;
        protected readonly List<TV> LoadedData;

        protected const int Delay = 1000;
        
        protected T data;
        protected int LoadedCount;

        public DataGetter(DataRequest request, MetalitixBridge metalitixBridge) : base(request, metalitixBridge)
        {
            ProjectData = request.MetalitixProjectData;
            PageQuery = request.Query;
            LoadedData = new List<TV>();
            
            EditorUtility.DisplayProgressBar("Loading initialization", $"Items loaded 0 / " + 1, 0);
        }

        public abstract Task<List<TV>> GetData();

        protected async Task HandleLoading(string progressTitle, int loadedCount, int totalCount)
        {
            LoadedCount += loadedCount;

            if (LoadedCount == 0)
            {
                MetalitixDebug.Log(this, "No UpdatedData from this period of time", true);
                return;
            }
                
            var progress = Mathf.InverseLerp(0, data.pagination.totalItemsCount, LoadedCount);
            EditorUtility.DisplayProgressBar(progressTitle, $"Items loaded {LoadedCount} / " + 
                                                            $"{totalCount}", progress);
            
            PageQuery = PageQuery.IncreasedPageRepeat();
            await Task.Yield();
        }
    }
}