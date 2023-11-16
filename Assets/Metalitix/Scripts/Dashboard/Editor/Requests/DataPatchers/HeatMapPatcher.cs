using System;
using System.Threading.Tasks;
using Metalitix.Core.Tools;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;

namespace Metalitix.Scripts.Dashboard.Editor.Requests.DataPatchers
{
#if UNITY_EDITOR
    public class HeatMapPatcher : DataPatcher<HeatMapSettingsData, MetalitixProjectData>
    {
        private readonly int _projectID;

        public HeatMapPatcher(int projectId, HeatMapSettingsData updatedData, DataRequest request, MetalitixBridge metalitixBridge) : base(updatedData, request, metalitixBridge)
        {
            _projectID = projectId;
            UpdatedData = updatedData;
        }

        public override async Task<MetalitixProjectData> UpdateAndReturn()
        {
            try
            {
                LoadedData = await MetalitixBridge.UpdateHeatMapSettings(_projectID, AuthToken, UpdatedData);
                LogSuccessful();
                return LoadedData;
            }
            catch (Exception e)
            {
                MetalitixDebug.LogError(this, e.Message);
                return null;
            }
        }

        public override async Task UpdateData()
        {
            try
            {
                LoadedData = await MetalitixBridge.UpdateHeatMapSettings(_projectID, AuthToken, UpdatedData);
                LogSuccessful();
            }
            catch (Exception e)
            {
                MetalitixDebug.LogError(this, e.Message);
            }
        }
    }
#endif
}