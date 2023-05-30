using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.MetalitixTools
{
    public class MetalitixBridge
    {
        private readonly string _serverUrl;
        
        private const string AuthEndPoint = "/auth/login";
        private const string AuthMeEndPoint = "/auth/me";

        public MetalitixBridge(string url)
        {
            _serverUrl = url;
        }

        public async Task<MetalitixUserData> Me(string token, CancellationToken cancellationToken = new ())
        {
            var url = BuildLink(_serverUrl, AuthMeEndPoint);
            var data = await WebRequestHelper.GetDataWithToken<MetalitixUserData>(url, token, cancellationToken);
            return data;
        }

        public async Task<MetalitixUserData> Login(MetalitixAuthorizationData authorizationData, CancellationToken cancellationToken = new ())
        {
            var url = BuildLink(_serverUrl, AuthEndPoint);
            var jsonPayload = JsonHelper.ToJson(authorizationData, NullValueHandling.Include);
            var data = await WebRequestHelper.PostDataWithPlayLoad<MetalitixUserData>(url, jsonPayload, cancellationToken);
            return data;
        }

        public async Task<HeatMapData> GetSessionHeatMapData(int projectId, string sessionId, string token, PageQuery query, CancellationToken cancellationToken = new ())
        {
            var sessionsEndPoint = $"/projects/{projectId}/xr-analytics/sessions/{sessionId}/heatmap";
            var url = BuildLink(_serverUrl, sessionsEndPoint, query);
            var data = await WebRequestHelper.GetDataWithToken<HeatMapData>(url, token, cancellationToken);
            return data;
        }

        public async Task<SessionsData> GetSessions(int projectId, string token, PageQuery query, CancellationToken cancellationToken = new ())
        {
            var sessionsEndPoint = $"/projects/{projectId}/xr-analytics/sessions";
            var url = BuildLink(_serverUrl, sessionsEndPoint, query);
            var data = await WebRequestHelper.GetDataWithToken<SessionsData>(url, token, cancellationToken);
            return data;
        }

        public async Task<HeatMapData> GetHeatmap(int projectId, string token, PageQuery query, CancellationToken cancellationToken = new ())
        {
            var heatMapEndPoint = $"/projects/{projectId}/xr-analytics/heatmap";
            var url = BuildLink(_serverUrl, heatMapEndPoint, query);
            var data = await WebRequestHelper.GetDataWithToken<HeatMapData>(url, token, cancellationToken);
            return data;
        }

        public async Task<HeatMapProcessingResponse[]> GetBackendHeatMapProcessing(int projectId, string token, CancellationToken cancellationToken = new ())
        {
            var heatMapEndPoint = $"/projects/{projectId}/heatmap-processing";
            var url = BuildLink(_serverUrl, heatMapEndPoint);
            var data = await WebRequestHelper.GetDataWithToken<HeatMapProcessingResponse[]>(url, token, cancellationToken);
            return data;
        }
        
        public async Task<HeatMapProcessingResponse[]> GetFilteredBackendHeatMapProcessing(int filterID, int projectId, string token, CancellationToken cancellationToken = new ())
        {
            var heatMapEndPoint = $"/projects/{projectId}/heatmap-processing/{filterID}";
            var url = BuildLink(_serverUrl, heatMapEndPoint);
            var data = await WebRequestHelper.GetDataWithToken<HeatMapProcessingResponse[]>(url, token, cancellationToken);
            return data;
        }

        public async Task<T> GetFromBucket<T>(string url, CancellationToken cancellationToken)
        {
            var data = await WebRequestHelper.GetData<T>(url, cancellationToken);
            return data;
        }

        public async Task<MetalitixProjectsData> GetUserProjectsList(string token, PageQuery query, CancellationToken cancellationToken = new ())
        {
            var projectsListEndPoint = $"/projects";
            var url = BuildLink(_serverUrl, projectsListEndPoint, query);
            var data = await WebRequestHelper.GetDataWithToken<MetalitixProjectsData>(url, token, cancellationToken);
            return data;
        }

        public async Task<ProjectFilters> GetFiltersListForProject(int projectID, string token, PageQuery query, CancellationToken cancellationToken = new ())
        {
            var projectsListEndPoint = $"/projects/{projectID}/filters-preset";
            var url = BuildLink(_serverUrl, projectsListEndPoint, query);
            var data = await WebRequestHelper.GetDataWithToken<ProjectFilters>(url, token, cancellationToken);
            return data;
        }

        public async Task<ProjectMember> GetProjectMemberData(int projectId, string token, CancellationToken cancellationToken = new ())
        {
            var projectsListEndPoint = $"/projects/{projectId}/members/me";
            var url = BuildLink(_serverUrl, projectsListEndPoint);
            var data = await WebRequestHelper.GetDataWithToken<ProjectMember>(url, token, cancellationToken);
            return data;
        }
        
        public async Task<WorkspaceMember> GetWorkspaceMemberData(int workSpaceId, string token, CancellationToken cancellationToken = new ())
        {
            var projectsListEndPoint = $"/workspaces/{workSpaceId}/members/me";
            var url = BuildLink(_serverUrl, projectsListEndPoint);
            var data = await WebRequestHelper.GetDataWithToken<WorkspaceMember>(url, token, cancellationToken);
            return data;
        }
        
        public async Task<MetalitixProjectData> UpdateHeatMapSettings(int projectId, string token, HeatMapSettingsData heatMapSettingsData, CancellationToken cancellationToken = new ())
        {
            var projectsListEndPoint = $"/projects/{projectId}/settings";
            var url = BuildLink(_serverUrl, projectsListEndPoint);
            var jsonPayload = JsonHelper.ToJson(heatMapSettingsData, NullValueHandling.Include);
            var data = await WebRequestHelper.PatchData<MetalitixProjectData>(url, token, jsonPayload, cancellationToken);
            return data;
        }
        
        private string BuildLink(string first, string second, PageQuery query = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(first);
            stringBuilder.Append(second);
            var url = stringBuilder.ToString();

            if (query == null) return url;
            
            var uriBuilder = new UriBuilder(url);
            uriBuilder.Query = query.GetQuery(uriBuilder);
            url = uriBuilder.ToString();
            return url;
        }
    }
}