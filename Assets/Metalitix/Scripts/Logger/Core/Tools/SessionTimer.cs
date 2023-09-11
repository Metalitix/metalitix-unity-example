using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Tools
{
    public class SessionTimer
    {
        private readonly float _estimatedAfkTime;
        private bool _isRunning;

        private TimerData _batchSendingTimer;
        private TimerData _startTimer;
        private TimerData _afkTimer;

        public event Action AfkTimeHasPassed;
        public event Action OnSendTimeReached;
        
        public TimerData StartTimer => _startTimer;
        public TimerData AfkTimer => _afkTimer;

        private const float TimeToSend = 5f;
        
        public SessionTimer(float estimatedAfkTime)
        {
            _estimatedAfkTime = estimatedAfkTime;
            _startTimer = new TimerData();
            _afkTimer = new TimerData();
            _batchSendingTimer = new TimerData();
        }

        public async Task LaunchTimer()
        {
            _isRunning = true;
            
            while (_isRunning)
            {
                _startTimer.AddElapsedTime(Time.deltaTime);
                _afkTimer.AddElapsedTime(Time.deltaTime);
                _batchSendingTimer.AddElapsedTime(Time.deltaTime);
                
                if (_afkTimer.elapsedTime >= _estimatedAfkTime)
                {
                    AfkTimeHasPassed?.Invoke();
                    ResetAfkTime();
                }
                
                if (_batchSendingTimer.elapsedTime >= TimeToSend)
                {
                    OnSendTimeReached?.Invoke();
                    ResetSendTime();
                }

                await Task.Yield();
            }
        }

        public void ResetSendTime()
        {
            _batchSendingTimer.ResetValues();
        }

        public void ResetAfkTime()
        {
            _afkTimer.ResetValues();
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        public void ResumeTimer()
        {
            _isRunning = true;
        }
    }
}