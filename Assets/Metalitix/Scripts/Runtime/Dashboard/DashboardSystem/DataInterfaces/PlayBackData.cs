using System;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces
{
    public class PlayBackData
    {
        private readonly PathPoint _startPoint;
        private readonly PathPoint _endPoint;

        private Vector3 _lerpedPos;
        private Quaternion _lerpedRotation;

        public PathPoint StartPoint => _startPoint;
        public PathPoint EndPoint => _endPoint;
        public TimeSpan SegmentDuration => _endPoint.GetTimeStamp - _startPoint.GetTimeStamp;

        public PlayBackData(PathPoint startPoint, PathPoint endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }
    }
}