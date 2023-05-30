using System.Collections.Generic;
using System.Linq;
using Metalitix.Scripts.Editor.Dashboard.Roles.Enums;

namespace Metalitix.Scripts.Editor.Dashboard.Roles.Processes
{
    public abstract class RestrictedProcess
    {
        protected readonly List<AuthRole> AuthRoles;
        protected readonly List<ScopedRole> ScopedRoles;

        public IEnumerator<AuthRole> authRoles => AuthRoles.GetEnumerator();
        public IEnumerator<ScopedRole> scopedRoles => ScopedRoles.GetEnumerator();
        
        public bool IsValid { get; protected set; }

        public RestrictedProcess(AuthRole authRole, ScopedRole workspaceRole, ScopedRole projectRole)
        {
            AuthRoles = new List<AuthRole>();
            ScopedRoles = new List<ScopedRole>();
            CreateAccess();
            IsValid = Evaluate(authRole, workspaceRole, projectRole);
        }

        protected abstract void CreateAccess();

        private bool Evaluate(AuthRole authRole, ScopedRole workspaceRole, ScopedRole projectRole)
        {
            if (AuthRoles.Any(r => r == authRole))
            {
                return true;
            }
            
            if (ScopedRoles.Any(r => r == workspaceRole))
            {
                return true;
            }
            
            if (ScopedRoles.Any(r => r == projectRole))
            {
                return true;
            }

            return false;
        }
    }
}