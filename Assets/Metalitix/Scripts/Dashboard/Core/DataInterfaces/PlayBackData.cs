using System;
using Metalitix.Core.Base;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Core.DataInterfaces
{
    public class PlayBackData
    {
        private readonly PathPoint _startPoint;
        private readonly PathPoint _endPoint;

        private bool isPauseArea;
        private bool isPassed;

        private Vector3 _lerpedPos;
        private Quaternion _lerpedRotation;
        private float _startTime;
        private float _endTime;

        public bool IsPassed => isPassed;
        public bool IsPauseArea => isPauseArea;
        public float StartTime => _startTime;
        public float EndTime => _endTime;
        public PathPoint StartPoint => _startPoint;
        public PathPoint EndPoint => _endPoint;
        public TimeSpan SegmentDuration => _endPoint.GetTimeStamp - _startPoint.GetTimeStamp;

        public PlayBackData(PathPoint startPoint, PathPoint endPoint, float startTime, float endTime)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;

            _startTime = startTime;
            _endTime = endTime;
        }

        public bool CheckInRange(float value)
        {
            return value >= _startTime && value <= _endTime;
        }

        public void SetPassed(bool value)
        {
            isPassed = value;
        }

        public void SetPauseArea()
        {
            isPauseArea = true;
        }

        public bool IsSimilar(PlayBackData data)
        {
            return _startPoint == data.StartPoint;
        }
    }
}