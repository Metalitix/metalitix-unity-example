using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Enums;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Sliders;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Elements.Toggles;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels
{
    [ExecuteInEditMode]
    public class TimeLinePanel : SessionPageInteractableUI
    {
        [Header("Interactable"), Space(2f)]
        [SerializeField] private PreviewSliderWithContainer slider;
        [SerializeField] private PreviewIconToggle playbackToggle;
        [SerializeField] private PreviewButton playbackSpeedButton;
        [SerializeField] private PreviewButton cameraModeButton;
        
        [Header("Text"), Space(2f)]
        [SerializeField] private TMP_Text currentTime;
        [SerializeField] private TMP_Text endTime;
        
        [Header("Panels"), Space(2f)]
        [SerializeField] private PlaybackPanel playbackPanel;
        [SerializeField] private CameraModePanel cameraModePanel;
        
        [Header("Prefabs")] 
        [SerializeField] private EventButton eventButton;
        
        private float _playBackSpeed;
        private float _currentTimeOnSlider;
        private TimeLineDestination _destination;
        private bool _isPaused = true;
        private bool _isPlaying;
        private CanvasScaler _canvasScaler;

        private PlayBackData _lastPlaybackData;

        private CancellationTokenSource _loopCancellation;
        private List<EventButton> _eventButtons;
        private Scene _targetScene;

        private Queue<float> _sliderRequest = new Queue<float>(1);

        private const float PlayBackSpeed = 1f;
        
        public event Action<Vector3, Vector3, MetalitixCameraData> OnChangeTimeLine;
        public event Action<PathPoint> OnEventClicked;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            SetVisible(false);
            slider.Initialize(DashboardCamera.TargetRenderCamera);
            playbackPanel.Initialize(DashboardCamera);
            cameraModePanel.Initialize(DashboardCamera);
            InitializeSubscribers();
            playbackToggle.Initialize();
        }

        public void SetTargetScene(Scene targetScene)
        {
            _targetScene = targetScene;
        }

        private void InitializeSubscribers()
        {
            slider.OnSliderValueChanged += SliderValueChanged;
            playbackSpeedButton.Button.onClick.AddListener(playbackPanel.ChangeVisible);
            cameraModeButton.Button.onClick.AddListener(OnCameraModeChanged);
            playbackPanel.OnPlaybackSpeedChanged += MultiplyPlaybackSpeed;
            playbackToggle.OnToggleValueChanged += Play;
        }
        
        private void OnCameraModeChanged()
        {
            cameraModePanel.ChangeVisible();
        }

        public override void SetSession(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            SetVisible(true);
            base.SetSession(pathPoints, eventPoints, sessionData);
            _loopCancellation?.Cancel();
            var time = TimeSpan.FromSeconds(SessionData.duration);
            var timerString = time.ToString(@"mm\:ss");
            var startPathPoint = PathPoints[0];
            OnChangeTimeLine?.Invoke(startPathPoint.GetPosition, startPathPoint.GetDirection, startPathPoint.GetMetalitixCameraData);

            SetTimeSpanFromTime(0f);
            SetSliderValueFromTime(0f);
            endTime.text = timerString;
            MultiplyPlaybackSpeed(PlayBackSpeed);
            cameraModePanel.InitSession();
            InitEventsOnSlider();
        }

        public void ResetPanel()
        {
            playbackToggle.SetState(false);
            _isPlaying = false;
            _isPaused = true;
            
            playbackPanel.SetDefault();
            _sliderRequest.Clear();
            slider.SetMinValue();
            _currentTimeOnSlider = 0f;
            
            if (_eventButtons != null)
            {
                foreach (var eButton in _eventButtons)
                {
                    DestroyImmediate(eButton.gameObject);
                }
                
                _eventButtons.Clear();
            }
            
            StopAllCoroutines();
        }

        private void InitEventsOnSlider()
        {
            _eventButtons = new List<EventButton>(EventPoints.Count);
            
            foreach (var eventPoint in EventPoints)
            {
                var button = (EventButton)PrefabUtility.InstantiatePrefab(eventButton, _targetScene);
                button.Initialize(eventPoint);
                button.OnEventClicked += InvokeEventClick;
                slider.SetTransformOnSlider(button.RectTransform, GetSliderValueByPoint(eventPoint));
                _eventButtons.Add(button);
            }
        }

        private void InvokeEventClick(PathPoint point)
        {
            OnEventClicked?.Invoke(point);
        }
        
        private void MultiplyPlaybackSpeed(float value)
        {
            _playBackSpeed = value;
            _destination = _playBackSpeed < 0 ? TimeLineDestination.Backward : TimeLineDestination.Forward;
            _loopCancellation?.Cancel();
        }

        public void SelectPoint(PathPoint point)
        {
            var sliderValue = GetSliderValueByPoint(point);
            var time = Mathf.Lerp(0, (float)TotalSeconds, sliderValue);
            _currentTimeOnSlider = time;
            SetTimeSpanFromTime(time);
            SetSliderValueFromTime(time);
        }

        private void SliderValueChanged(float value)
        {
            if (_sliderRequest.Count != 0)
            {
                _sliderRequest.Dequeue();
            }
            
            _sliderRequest.Enqueue(value);

            if (!_isPlaying)
            {
                StartCoroutine(CheckSliderValueChanged());
            }
        }

        private IEnumerator CheckSliderValueChanged()
        {
            if (_sliderRequest.Count == 0) yield break;
            
            var sliderValue = _sliderRequest.Dequeue();
            var timeValue =  0 + (float)(TotalSeconds - 0) * Mathf.Clamp01(sliderValue);
            SetTimeSpanFromTime(timeValue);
            var t = timeValue / (float)TotalSeconds;
            SetPointsByTime(t, out var startIndex);
            SetPositionAndRotation(t, startIndex);
            _currentTimeOnSlider = timeValue;
            SetSliderValueFromTime(_currentTimeOnSlider);
            yield return null;
        }

        private IEnumerator PlayPath()
        {
            if (PathPoints == null || PathPoints.Count == 0) yield break;

            if (_lastPlaybackData == null)
            {
                SetPointsByTime(0, out var startIndex);
                SetPositionAndRotation(0, startIndex);
            }
            else
            {
                SetPointsByTime(_currentTimeOnSlider, out var startIndex);
                SetPositionAndRotation(_currentTimeOnSlider, startIndex);
            }
            
            _isPlaying = true;
            
            yield return LerpBetweenPoints();
            
            playbackToggle.Interact();
            _isPlaying = false;
            _isPaused = false;
        }

        private IEnumerator LerpBetweenPoints()
        {
            if(_lastPlaybackData == null) yield break;
            
            _loopCancellation = new CancellationTokenSource();

            while (_currentTimeOnSlider <= (float)TotalSeconds || _currentTimeOnSlider >= 0 && !_loopCancellation.IsCancellationRequested)
            {
                if (!_isPaused)
                {
                    _currentTimeOnSlider += Time.deltaTime * _playBackSpeed;
                    float t = _currentTimeOnSlider / (float)TotalSeconds;

                    if (!SetPointsByTime(t, out var startIndex))
                    {
                        AdjustTime();
                        yield break;
                    }
                    
                    yield return CheckSliderValueChanged();
                    SetPositionAndRotation(t, startIndex);
                    SetTimeSpanFromTime(_currentTimeOnSlider);
                    SetSliderValueFromTime(_currentTimeOnSlider);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void AdjustTime()
        {
            if (_currentTimeOnSlider < 0)
            {
                SetTimeSpanFromTime(0f);
                slider.SetMinValue();
            }
            else if(_currentTimeOnSlider > TotalSeconds)
            {
                SetTimeSpanFromTime((float)TotalSeconds);
                slider.SetMaxValue();
            }
        }

        private void SetPositionAndRotation(float t, int startIndex)
        {
            var startPosition = _lastPlaybackData.StartPoint.GetPosition;
            var endPosition = _lastPlaybackData.EndPoint.GetPosition;
            var startDirection = _lastPlaybackData.StartPoint.GetDirection;
            var endDirection = _lastPlaybackData.EndPoint.GetDirection;
            var cameraData = _lastPlaybackData.EndPoint.GetMetalitixCameraData;
            var segmentT = (t - (float)startIndex / (PathPoints.Count - 1)) * (PathPoints.Count - 1);
            var lerpedPos = Vector3.Lerp(startPosition, endPosition, segmentT);
            var lerpedRotation = Quaternion.Slerp(Quaternion.Euler(startDirection),
                Quaternion.Euler(endDirection), segmentT);
            
            OnChangeTimeLine?.Invoke(lerpedPos, lerpedRotation.eulerAngles, cameraData);
        }

        private void SetPointsByPoint(PathPoint point)
        {
            var index = PathPoints.IndexOf(point);
            int endIndex = _destination switch
            {
                TimeLineDestination.Forward => index + 1,
                TimeLineDestination.Backward => index - 1,
                _ => throw new ArgumentOutOfRangeException()
            };

            PathPoint startPoint = PathPoints[index];
            PathPoint endPoint;
            
            if (endIndex > PathPoints.Count || endIndex < 0)
            {
                endPoint = PathPoints[index];
            }
            else
            {
                endPoint = PathPoints[endIndex];
            }
            
            _lastPlaybackData = new PlayBackData(startPoint, endPoint);
            OnChangeTimeLine?.Invoke(point.GetPosition, point.GetDirection, point.GetMetalitixCameraData);
        }
        
        private bool SetPointsByTime(float time, out int index)
        {
            int startIndex = Mathf.FloorToInt(time * (PathPoints.Count - 1));
            int endIndex = Mathf.CeilToInt(time * (PathPoints.Count - 1));
            
            if (startIndex < 0 || endIndex > PathPoints.Count - 1)
            {
                index = PathPoints.Count;
                return false;
            }
            
            PathPoint startPoint = PathPoints[startIndex];
            PathPoint endPoint = PathPoints[endIndex];
            _lastPlaybackData = new PlayBackData(startPoint, endPoint);
            index = startIndex;
            return true;
        }
        
        private float GetSliderValueByPoint(PathPoint point)
        {
            DateTime minTimeStamp = PathPoints.Min(p => p.GetTimeStamp);
            DateTime maxTimeStamp = PathPoints.Max(p => p.GetTimeStamp);
            DateTime timeStamp = point.GetTimeStamp;

            var value = (float)(timeStamp - minTimeStamp).TotalSeconds / (float)(maxTimeStamp - minTimeStamp).TotalSeconds;
            return value;
        }

        private void SetTimeSpanFromTime(float time)
        {
            TimeSpan elapsedSpan = TimeSpan.FromSeconds(time);
            string timerString = elapsedSpan.ToString(@"mm\:ss");
            currentTime.text = timerString;
        }

        private void SetSliderValueFromTime(float time)
        {
            float normalValue = Mathf.InverseLerp(0, (float)TotalSeconds, time);
            normalValue = Mathf.Clamp01(normalValue);
            slider.SetValue(normalValue);
        }

        private void PausePlayback()
        {
            _isPaused = true;
            _loopCancellation?.Cancel();
        }

        private void ResumePlayback()
        {
            _isPaused = false;
        }

        private void Play(bool state)
        {
            if (!state)
            {
                PausePlayback();
                return;
            }
                
            ResumePlayback();

            if(!_isPlaying)
                StartCoroutine(PlayPath());
        }
    }
}