using System;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Extensions;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataPatchers
{
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
                MetalitixDebug.Log(this, e.Message, true);
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
                MetalitixDebug.Log(this, e.Message, true);
            }
        }
    }
}