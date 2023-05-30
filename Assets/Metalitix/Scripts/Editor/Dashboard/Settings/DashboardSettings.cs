using System;
using Metalitix.Scripts.Editor.Configs;
using Metalitix.Scripts.Editor.Dashboard.Roles.Enums;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Dashboard.Settings
{
    [CreateAssetMenu(fileName = "Metalitix/DashboardSettings", menuName = "Metalitix/DashboardSettings", order = 0)]
    public class DashboardSettings : ScriptableObject
    {
        [SerializeField, HideInInspector] private string login;
        [SerializeField, HideInInspector] private string password;
        [SerializeField, HideInInspector] private string token;
        [SerializeField, HideInInspector] private AuthRole authRole;
        [SerializeField, HideInInspector] private ScopedRole projectRole;
        [SerializeField, HideInInspector] private ScopedRole workspaceRole;
        [SerializeField, HideInInspector] private MetalitixProjectData targetProject;
        [SerializeField, HideInInspector] private MetalitixUserData response;
        [SerializeField, HideInInspector] private ProjectMember projectMember;
        [SerializeField, HideInInspector] private WorkspaceMember workspaceMember;

        public string Login
        {
            get => login;
            set => login = String.IsNullOrEmpty(value) ? null : value.Trim();
        }

        public string Password
        {
            get => password;
            set => password = String.IsNullOrEmpty(value) ? null : value.Trim();
        }
        
        public string AuthToken => token;
        public AuthRole AuthRole => authRole;
        public ScopedRole ProjectRole => projectRole;
        public ScopedRole WorkspaceRole => workspaceRole;
        public string UserID => response?.id;
        public string UserFullName => $"{response.firstName} {response.lastName}";
        public int ProjectID => targetProject.id;
        public ProjectMember ProjectMember => projectMember;
        public WorkspaceMember WorkspaceMember => workspaceMember;
        public MetalitixProjectData TargetProject => targetProject;

        public bool ValidateAuth()
        {
            return !String.IsNullOrEmpty(token);
        }
        
        public void SetToken(string authToken)
        {
            token = authToken;
        }

        public void SetAuthResponse(MetalitixUserData userData)
        {
            if(userData == null) return;
         
            response = userData;

            if (Enum.TryParse<AuthRole>(userData.role, out var aRole))
            {
                authRole = aRole;
            }
        }

        public void SetTargetProject(MetalitixProjectData metalitixProjectData)
        {
            if(metalitixProjectData == null) return;

            targetProject = metalitixProjectData;
        }

        public void SetProjectMember(ProjectMember projectMemberData)
        {
            if(projectMemberData == null) return;

            projectMember = projectMemberData;
            
            if (Enum.TryParse<ScopedRole>(projectMember.role, out var sRole))
            {
                projectRole = sRole;
            }
        }
        
        public void SetWorkspaceMember(WorkspaceMember workspaceMemberData)
        {
            if(workspaceMemberData == null) return;

            workspaceMember = workspaceMemberData;
            
            if (Enum.TryParse<ScopedRole>(workspaceMember.role, out var sRole))
            {
                workspaceRole = sRole;
            }
        }
        
        public void ClearResponse()
        {
            response = null;
            token = String.Empty;
            login = String.Empty;
            password = String.Empty;
            EditorPrefs.DeleteKey(EditorConfig.AuthEditorSave);
        }
    }
}