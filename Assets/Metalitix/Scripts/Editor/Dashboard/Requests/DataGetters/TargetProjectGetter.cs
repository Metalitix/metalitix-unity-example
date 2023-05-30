using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using UnityEditor;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataGetters
{
    public class TargetProjectGetter : DataGetter<MetalitixProjectsData, MetalitixProjectData>
    {
        private readonly string _targetApiKey;
        
        public MetalitixProjectData MetalitixProjectData { get; private set; }
        
        public ProjectMember ProjectMember { get; private set; }
        public WorkspaceMember WorkspaceMember { get; private set; }
        
        public TargetProjectGetter(string targetApiKey, DataRequest request, MetalitixBridge metalitixBridge) : base(request, metalitixBridge)
        {
            _targetApiKey = targetApiKey;
        }

        public override async Task<List<MetalitixProjectData>> GetData()
        {
            do
            {
                data = await MetalitixBridge.GetUserProjectsList(AuthToken, PageQuery, Source.Token);
                
                if(data == null) break;
                
                foreach (var item in data.items)
                {
                    LoadedData.Add(item);

                    if (item.scriptApiKey.Equals(_targetApiKey))
                    {
                        MetalitixProjectData = item;

                        await GetProjectMember();
                        await GetWorkspaceMember();
                    }
                }
                
                await HandleLoading($"Projects list loading for user", 
                    data.items.Length, data.pagination.totalItemsCount);
                
            } 
            while (LoadedCount < data.pagination.totalItemsCount && !Source.IsCancellationRequested);

            await Task.Delay(Delay);
            EditorUtility.ClearProgressBar();
            return LoadedData;
        }

        private async Task GetWorkspaceMember()
        {
            try
            {
                var workspaceMemberData =
                    await MetalitixBridge.GetWorkspaceMemberData(MetalitixProjectData.workspaceId, AuthToken, Source.Token);
                WorkspaceMember = workspaceMemberData;
            }
            catch (Exception e)
            {
                MetalitixDebug.Log(this, e.Message);
                throw;
            }
        }

        private async Task GetProjectMember()
        {
            try
            {
                var projectMemberData =
                    await MetalitixBridge.GetProjectMemberData(MetalitixProjectData.id, AuthToken, Source.Token);
                ProjectMember = projectMemberData;
            }
            catch (Exception e)
            {
                MetalitixDebug.Log(this, e.Message);
                throw;
            }
        }
    }
}