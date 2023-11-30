using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Dashboard.Core.DataInterfaces;
using Metalitix.Scripts.Dashboard.Tools;
using Metalitix.Scripts.Logger.Core.Base;
using Metalitix.Scripts.Preview.Base;
using Metalitix.Scripts.Preview.Elements.Sliders;
using Metalitix.Scripts.Preview.Elements.Toggles;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using Metalitix.Editor.Enums;
#endif

namespace Metalitix.Scripts.Dashboard.Panels
{
    [ExecuteInEditMode]
    public class TimeLinePanel : SessionPageInteractableUI
    {
        [Header("Interactable"), Space(2f)]
#if UNITY_EDITOR
        [SerializeField] private PreviewSliderWithContainer slider;
#endif
        [SerializeField] private PreviewIconToggle playbackToggle;
        [SerializeField] private PreviewButton playbackSpeedButton;
        [SerializeField] private PreviewButton cameraModeButton;
        [SerializeField] private RectTransform containerForPauses;

        [Header("Text"), Space(2f)]
        [SerializeField] private TMP_Text currentTime;
        [SerializeField] private TMP_Text endTime;

        [Header("Panels"), Space(2f)]
        [SerializeField] private PlaybackPanel playbackPanel;
        [SerializeField] private CameraModePanel cameraModePanel;

        [Header("Prefabs")]
        [SerializeField] private EventButton eventButton;
        [SerializeField] private PauseArea pauseAreaPrefab;

        private float _playBackSpeed;
        private float _currentTimeOnSlider;
#if UNITY_EDITOR
        private TimeLineDestination _destination;
 #endif
        private bool _isPaused = true;
        private bool _isPlaying;
        private CanvasScaler _canvasScaler;
        private List<PauseArea> _currentPauseAreas;
        private List<PlayBackData> _timeLine;
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
#if UNITY_EDITOR
            slider.Initialize(DashboardCamera.TargetRenderCamera);
#endif
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
#if UNITY_EDITOR
            slider.OnSliderValueChanged += SliderValueChanged;
#endif
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
            var startPathPoint = PathPoints[0];
            OnChangeTimeLine?.Invoke(startPathPoint.GetPosition, startPathPoint.GetDirection, startPathPoint.GetMetalitixCameraData);
            var timeSpan = GetTime();
            SetDuration(timeSpan);
            var totalSeconds = timeSpan.ToString(@"mm\:ss");

            SetTimeSpanFromTime(0f);
            SetSliderValueFromTime(0f);
            endTime.text = totalSeconds;
            MultiplyPlaybackSpeed(PlayBackSpeed);
            cameraModePanel.InitSession();
            InitEventsOnSlider();
            InitializePauseAreas();
            CreateTimeLine();
        }

        private void CreateTimeLine()   
        {
            _timeLine = new List<PlayBackData>();
            var currentTime = 0f;

            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                var startPoint = PathPoints[i];
                var endPoint = PathPoints[i + 1];
                var totalSegmentSeconds = (float)(endPoint.GetTimeStamp - startPoint.GetTimeStamp).TotalSeconds;
                var playBackData = new PlayBackData(startPoint, endPoint, currentTime, currentTime + totalSegmentSeconds);

                _timeLine.Add(playBackData);
                currentTime += totalSegmentSeconds;
            }
        }

        private TimeSpan GetTime()
        {
            var firstTimeStamp = PathPoints[0].GetUnixTimeStamp;
            var secondTimeStamp = PathPoints.Last().GetUnixTimeStamp;
            var timeSpan = TimeSpan.FromSeconds(secondTimeStamp - firstTimeStamp);
            return timeSpan;
        }

        private void InitializePauseAreas()
        {
            _currentPauseAreas = new List<PauseArea>();

            for (var i = 0; i < PathPoints.Count; i++)
            {
                var pausePathPoint = PathPoints[i];

                if (pausePathPoint.TypeOfEvent.Equals(MetalitixEventType.SessionPause))
                {
                    bool isFinded = false;

                    for (int y = i; y < PathPoints.Count; y++)
                    {
                        var resumePathPoint = PathPoints[y];

                        if (resumePathPoint.TypeOfEvent.Equals(MetalitixEventType.SessionResume))
                        {
                            var pauseArea = InstantiatePauseArea(pausePathPoint, resumePathPoint);
                            _currentPauseAreas.Add(pauseArea);
                            isFinded = true;
                            break;
                        }
                    }

                    if (!isFinded)
                    {
                        PauseArea pauseArea = InstantiatePauseArea(pausePathPoint, PathPoints.Last());
                        _currentPauseAreas.Add(pauseArea);
                        break;
                    }
                }
            }
        }

        private PauseArea InstantiatePauseArea(PathPoint pause, PathPoint resume)
        {
            var pauseAreaTmp = Instantiate(pauseAreaPrefab, containerForPauses);
            pauseAreaTmp.Initialize(pause, resume);
            pauseAreaTmp.SetPauseValue(GetSliderValueByPoint(pause));
            pauseAreaTmp.SetResumeValue(GetSliderValueByPoint(resume));
            var width = 0f;
            var posFirst = 0f;
            var posSecond = 0f;
#if UNITY_EDITOR
            width = slider.CalculateWidthBetweenValues(pauseAreaTmp.PauseSliderValue, pauseAreaTmp.ResumeSliderValue);
            posFirst = slider.GetPositionOnSlider(pauseAreaTmp.PauseSliderValue);
            posSecond = slider.GetPositionOnSlider(pauseAreaTmp.ResumeSliderValue);
#endif
            var middlePos = (posSecond + posFirst) / 2;
            pauseAreaTmp.SetWidth(width);
            pauseAreaTmp.SetPosX(middlePos);
            _currentPauseAreas.Add(pauseAreaTmp);
            return pauseAreaTmp;
        }

        public void ResetPanel()
        {
            playbackToggle.SetState(false);
            _isPlaying = false;
            _isPaused = true;

            playbackPanel.SetDefault();
            _sliderRequest.Clear();
#if UNITY_EDITOR
            slider.SetMinValue();
#endif
            _currentTimeOnSlider = 0f;

            if (_eventButtons != null)
            {
                foreach (var eButton in _eventButtons)
                {
                    DestroyImmediate(eButton.gameObject);
                }

                _eventButtons.Clear();
            }

            if (_currentPauseAreas != null)
            {
                foreach (var pauseArea in _currentPauseAreas)
                {
                    if (pauseArea != null)
                    {
                        DestroyImmediate(pauseArea.gameObject);
                    }
                }

                _currentPauseAreas.Clear();
            }

            StopAllCoroutines();
        }

        private void InitEventsOnSlider()
        {
            _eventButtons = new List<EventButton>(EventPoints.Count);

#if UNITY_EDITOR
            foreach (var eventPoint in EventPoints)
            {
                var button = (EventButton)PrefabUtility.InstantiatePrefab(eventButton, _targetScene);
                button.Initialize(eventPoint);
                button.OnEventClicked += InvokeEventClick;
#if UNITY_EDITOR
                slider.SetTransformOnSlider(button.RectTransform, GetSliderValueByPoint(eventPoint));
#endif
                _eventButtons.Add(button);
            }
#endif
        }

        private void InvokeEventClick(PathPoint point)
        {
            OnEventClicked?.Invoke(point);
        }

        private void MultiplyPlaybackSpeed(float value)
        {
            ClearPassedData();
            _playBackSpeed = value;
#if UNITY_EDITOR
            _destination = _playBackSpeed < 0 ? TimeLineDestination.Backward : TimeLineDestination.Forward;
#endif
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

            ClearPassedData();
            var sliderValue = _sliderRequest.Dequeue();
            var timeValue = 0 + (float)(TotalSeconds - 0) * Mathf.Clamp01(sliderValue);
            SetTimeSpanFromTime(timeValue);
            GetPointByTime(timeValue);
            _currentTimeOnSlider = timeValue;
            SetPositionAndRotation(_currentTimeOnSlider, false);
            SetSliderValueFromTime(_currentTimeOnSlider);
            yield return null;
        }

        private IEnumerator PlayPath()
        {
            if (PathPoints == null || PathPoints.Count == 0) yield break;

            if (_lastPlaybackData == null)
            {
                GetPointByTime(0);
                SetPositionAndRotation(0, false);
            }
            else
            {
                GetPointByTime(_currentTimeOnSlider);
                SetPositionAndRotation(_currentTimeOnSlider, false);
            }

            _isPlaying = true;

            StartCoroutine(PlayAnimation());
            
            yield return LerpBetweenPoints();
            
            StopCoroutine(PlayAnimation());
            playbackToggle.Interact();
            _isPlaying = false;
            _isPaused = false;
            ClearPassedData();
        }

        private IEnumerator LerpBetweenPoints()
        {
            if (_lastPlaybackData == null) yield break;

            _loopCancellation = new CancellationTokenSource();

            while (_currentTimeOnSlider <= (float)TotalSeconds || _currentTimeOnSlider >= 0 && !_loopCancellation.IsCancellationRequested)
            {
                if (!_isPaused)
                {
                    UpdateTime();
                    _lastPlaybackData.SetPassed(true);
                        
                    if (!GetPointByTime(_currentTimeOnSlider))
                    {
                        Sync(true);
                        AdjustTime();
                        yield break;
                    }

                    yield return CheckSliderValueChanged();
                    Sync();
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void UpdateTime()
        {
            if (!_lastPlaybackData.IsPauseArea || _lastPlaybackData.IsPassed)
            {
                _currentTimeOnSlider += Time.deltaTime * _playBackSpeed;
            }
            else
            {
                if (_playBackSpeed < 0)
                {
                    _currentTimeOnSlider -= (float)_lastPlaybackData.SegmentDuration.TotalSeconds;
                }
                else
                {
                    _currentTimeOnSlider += (float)_lastPlaybackData.SegmentDuration.TotalSeconds;
                }
            }
        }

        private void Sync(bool isEnd = false)
        {
            SetPositionAndRotation(_currentTimeOnSlider, isEnd);
            SetTimeSpanFromTime(_currentTimeOnSlider);
            SetSliderValueFromTime(_currentTimeOnSlider);
        }

        private IEnumerator PlayAnimation()
        {
            while (true)
            {
                if (!_isPaused)
                {
                    foreach (var data in _lastPlaybackData.EndPoint.GetAnimationData)
                    {
                        MetalitixScene.UpdateAnimation(data.name, data.progress, data.loop);
                    }
                }
                
                yield return null;
            }
        }

        private void AdjustTime()
        {
            if (_currentTimeOnSlider < 0 + 0.5f)
            {
                SetTimeSpanFromTime(0f);
#if UNITY_EDITOR
                slider.SetMinValue();
#endif
                return;
            }
            
            if (_currentTimeOnSlider > TotalSeconds - 0.5f)
            {
                SetTimeSpanFromTime((float)TotalSeconds);
#if UNITY_EDITOR
                slider.SetMaxValue();
#endif
            }
        }

        private void SetPositionAndRotation(float time, bool isEnd)
        {
            var startPosition = _lastPlaybackData.StartPoint.GetPosition;
            var endPosition = _lastPlaybackData.EndPoint.GetPosition;
            var startDirection = _lastPlaybackData.StartPoint.GetDirection;
            var endDirection = _lastPlaybackData.EndPoint.GetDirection;
            var cameraData = _lastPlaybackData.EndPoint.GetMetalitixCameraData;
            var segmentT = isEnd ? _lastPlaybackData.EndTime : Mathf.InverseLerp(_lastPlaybackData.StartTime, _lastPlaybackData.EndTime, time);
            var lerpedPos = Vector3.Lerp(startPosition, endPosition, segmentT);
            var lerpedRotation = Quaternion.Slerp(Quaternion.Euler(startDirection),
                Quaternion.Euler(endDirection), segmentT);

            OnChangeTimeLine?.Invoke(lerpedPos, lerpedRotation.eulerAngles, cameraData);
        }

        private bool GetPointByTime(float time)
        {
            var tempData = GetPlaybackDataByTime(time);

            if (tempData == null)
                return false;

            if (tempData.StartPoint.TypeOfEvent.Equals(MetalitixEventType.SessionPause))
            {
                tempData.SetPauseArea();
            }

            _lastPlaybackData = tempData;
            return true;
        }

        private void ClearPassedData()
        {
            if(_timeLine == null) return;

            foreach (var data in _timeLine)
            {
                data.SetPassed(false);
            }
        }

        private PlayBackData GetPlaybackDataByTime(float time)
        {
            foreach (var playBackData in _timeLine)
            {
                if (playBackData.CheckInRange(time))
                {
                    return playBackData;
                }
            }

            return null;
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
#if UNITY_EDITOR
            slider.SetValue(normalValue);
#endif
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

        private int GetPointIndex(PathPoint point)
        {
            return PathPoints.IndexOf(point);
        }

        private void Play(bool state)
        {
            if (!state)
            {
                PausePlayback();
                return;
            }

            ResumePlayback();

            if (!_isPlaying)
                StartCoroutine(PlayPath());
        }
    }
}