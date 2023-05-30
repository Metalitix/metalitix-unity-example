using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.DashboardStates
{
    public abstract class DashboardState
    {
        protected readonly DashboardContainer DashboardContainer;
        
        public DashboardState(DashboardContainer dashboardContainer)
        {
            DashboardContainer = dashboardContainer;
        }

        public abstract void Enter();
        public abstract void Exit();
    }
}