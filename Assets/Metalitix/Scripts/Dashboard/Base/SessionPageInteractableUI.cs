using System;
using System.Collections.Generic;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Runtime;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Base
{
    [ExecuteInEditMode]
    public abstract class SessionPageInteractableUI : PreviewInteractableUI
    {
        protected List<PathPoint> PathPoints;
        protected List<PathPoint> EventPoints;
        protected SessionData SessionData;

        private TimeSpan _duration;

        protected double TotalSeconds => _duration.TotalSeconds;
        protected double TotalActiveSeconds => TimeSpan.FromSeconds(SessionData.duration).TotalSeconds;

        public virtual void SetSession(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            PathPoints = pathPoints;
            SessionData = sessionData;
            EventPoints = eventPoints;
        }

        protected void SetDuration(TimeSpan timeSpan)
        {
            _duration = timeSpan;
        }
    }
}