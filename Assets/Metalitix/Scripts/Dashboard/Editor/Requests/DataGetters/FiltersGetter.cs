using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;
using UnityEditor;

namespace Metalitix.Scripts.Dashboard.Editor.Requests.DataGetters
{
#if UNITY_EDITOR
    public class FiltersGetter : DataGetter<ProjectFilters, ProjectFilter>
    {
        public FiltersGetter(DataRequest request, MetalitixBridge metalitixBridge) : base(request, metalitixBridge)
        {

        }

        public override async Task<List<ProjectFilter>> GetData()
        {
            do
            {
                data = await MetalitixBridge.GetFiltersListForProject(ProjectData.id, AuthToken, PageQuery, Source.Token);

                if (data == null) break;

                foreach (var item in data.items)
                {
                    LoadedData.Add(item);
                }

                await HandleLoading($"Filters loading for project {ProjectData.title}",
                    data.items.Length, data.pagination.totalItemsCount);
            }
            while (LoadedCount < data.pagination.totalItemsCount && !Source.IsCancellationRequested);

            await Task.Delay(Delay);
            EditorUtility.ClearProgressBar();
            return LoadedData;
        }

        public List<FilterWrapper> GetFilterWrappers()
        {
            var wrapperList = new List<FilterWrapper>();

            foreach (var projectFilter in LoadedData)
            {
                var filter = new FilterWrapper(projectFilter.id, projectFilter.name);
                wrapperList.Add(filter);
            }

            return wrapperList;
        }
    }
#endif
}