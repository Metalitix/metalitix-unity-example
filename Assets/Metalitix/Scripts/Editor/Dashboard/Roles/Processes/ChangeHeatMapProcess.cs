using Metalitix.Scripts.Editor.Dashboard.Roles.Enums;

namespace Metalitix.Scripts.Editor.Dashboard.Roles.Processes
{
    public class ChangeHeatMapProcess : RestrictedProcess
    {
        public ChangeHeatMapProcess(AuthRole authRole, ScopedRole workspaceRole, ScopedRole projectRole)
            : base(authRole, workspaceRole, projectRole)
        {
            
        }

        protected override void CreateAccess()
        {
            AuthRoles.Add(AuthRole.admin_user);
            AuthRoles.Add(AuthRole.master_admin);
            ScopedRoles.Add(ScopedRole.admin);
            ScopedRoles.Add(ScopedRole.developer);
        }
    }
}