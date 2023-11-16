using Metalitix.Scripts.Dashboard.Base;

namespace Metalitix.Scripts.Dashboard.Core.States.DashboardStates
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