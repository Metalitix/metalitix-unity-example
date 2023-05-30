using System;
using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base
{
    [ExecuteInEditMode]
    public abstract class SessionPageInteractableUI : PreviewInteractableUI
    {
        protected List<PathPoint> PathPoints;
        protected List<PathPoint> EventPoints;
        protected SessionData SessionData;

        protected double TotalSeconds => TimeSpan.FromSeconds(SessionData.duration).TotalSeconds;

        public virtual void SetSession(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            PathPoints = pathPoints;
            SessionData = sessionData;
            EventPoints = eventPoints;
        }
    }
}