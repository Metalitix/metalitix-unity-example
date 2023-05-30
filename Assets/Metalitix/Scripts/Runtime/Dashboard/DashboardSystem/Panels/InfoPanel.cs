using System;
using System.Collections.Generic;
using System.Linq;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.DataInterfaces;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Interactables.Base;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Tools;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using TMPro;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(LookAtDashboardCamera))]
    [RequireComponent(typeof(ScaleWithDashboardCameraDistance))]
    public class InfoPanel : SessionPageInteractableUI
    {
        [SerializeField] private LookAtDashboardCamera lookAtDashboardCamera;
        [SerializeField] private ScaleWithDashboardCameraDistance scaleWithDashboard;
        [SerializeField] private Vector3 offset;

        [Header("Position"), Space(2)] 
        [SerializeField] private TMP_Text posX;
        [SerializeField] private TMP_Text posY;
        [SerializeField] private TMP_Text posZ;
        
        [Header("Direction"), Space(2)] 
        [SerializeField] private TMP_Text dirX;
        [SerializeField] private TMP_Text dirY;
        [SerializeField] private TMP_Text dirZ;
        
        [Header("Event")]
        [SerializeField] private TMP_Text eventName;

        [Header("ID"), Space(2)] 
        [SerializeField] private TMP_Text currentPosID;
        [SerializeField] private TMP_Text maxPosID;
        [SerializeField] private TMP_Text currentEventID;
        [SerializeField] private TMP_Text maxEventID;
        [SerializeField] private GameObject countPanel;

        [Header("Buttons"), Space(2)] 
        [SerializeField] private PreviewButton backPosButton;
        [SerializeField] private PreviewButton nextPosButton;
        [SerializeField] private PreviewButton backEventButton;
        [SerializeField] private PreviewButton nextEventButton;
        [SerializeField] private PreviewButton close;
        
        [Header("Bottom")]
        [SerializeField] private TMP_Text date;

        private PathPoint _currentPathPoint;

        public event Action<PathPoint> OnPointSelected;

        public override void Initialize(DashboardCamera dashboardCamera)
        {
            base.Initialize(dashboardCamera);
            SetVisible(false);
            lookAtDashboardCamera.Initialize(dashboardCamera);
            scaleWithDashboard.Initialize(transform, dashboardCamera);
            close.Button.onClick.AddListener(() => SetVisible(false));
        }
        
        public override void SetSession(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            base.SetSession(pathPoints, eventPoints, sessionData);
            
            foreach (var pathPoint in PathPoints)
            {
                pathPoint.OnInteract += Show;
            }
            
            backPosButton.Button.onClick.AddListener(BackPos);
            nextPosButton.Button.onClick.AddListener(NextPos);
            backEventButton.Button.onClick.AddListener(BackEvent);
            nextEventButton.Button.onClick.AddListener(NextEvent);
        }

        public void Close()
        {
            if (PathPoints != null)
            {
                foreach (var pathPoint in PathPoints)
                {
                    pathPoint.OnInteract -= Show;
                }
            }
            
            backPosButton.Button.onClick.RemoveListener(BackPos);
            nextPosButton.Button.onClick.RemoveListener(NextPos);
            backEventButton.Button.onClick.RemoveListener(BackEvent);
            nextEventButton.Button.onClick.RemoveListener(NextEvent);

            SetVisible(false);
        }
        
        public void Show(PathPoint pathPoint)
        {
            _currentPathPoint = pathPoint;
            SetEventUpdate();
            SetPositionUpdate();
            OnPointSelected?.Invoke(pathPoint);
            date.text = pathPoint.GetDate;
            SetVisible(true);
        }

        private void SetPositionUpdate()
        {
            var position = _currentPathPoint.GetPosition;
            
            transform.position = position + offset;

            var direction = _currentPathPoint.GetDirection;
            int maxPoses = PathPoints.Count;
            int posId = _currentPathPoint.GetID + 1;

            if (posId == 1)
            {
                backPosButton.SetInteractable(false);
            }
            else
            {
                backPosButton.SetInteractable(true);
                nextPosButton.SetInteractable(posId != maxPoses);
            }
            
            currentPosID.text = posId.ToString();
            maxPosID.text = maxPoses.ToString();
            
            posX.text = position.x.ToString("n2");
            posY.text = position.y.ToString("n2");
            posZ.text = position.z.ToString("n2");
            
            dirX.text = direction.x.ToString("n2");
            dirY.text = direction.y.ToString("n2");
            dirZ.text = direction.z.ToString("n2");
        }

        private void SetEventUpdate()
        {
            var nameOfEvent = _currentPathPoint.GetEventName;
            
            if (String.IsNullOrEmpty(nameOfEvent))
            {
                nameOfEvent = "None";
                ShowEventCountPanel(false);
                backEventButton.SetInteractable(false);
                nextEventButton.SetInteractable(EventPoints.Count != 0);
            }
            else
            {
                int eventId = _currentPathPoint.GetEventID.Value;

                if (eventId == 1)
                {
                    backEventButton.SetInteractable(false);
                }
                else
                {
                    backEventButton.SetInteractable(true);
                    var state = eventId != EventPoints.Count;
                    nextEventButton.SetInteractable(state);
                    
                }
                
                currentEventID.text = eventId.ToString();
                ShowEventCountPanel(true);
            }
            
            eventName.text = nameOfEvent;
            
            if (EventPoints.Count > 0)
            {
                maxEventID.text = EventPoints.Count.ToString();
            }
        }
        
        private void NextPos()
        {
            if(_currentPathPoint == null) return;

            var targetId = _currentPathPoint.GetID + 1;
            
            if(targetId > PathPoints.Count) return;
            
            var point = PathPoints[targetId];
            Show(point);
        }

        private void BackPos()
        {
            if(_currentPathPoint == null) return;
            
            var targetId = _currentPathPoint.GetID - 1;
            
            if(targetId < 0) return;
            
            var point = PathPoints[targetId];
            Show(point);
        }

        private void NextEvent()
        {
            if(_currentPathPoint == null) return;

            var id = _currentPathPoint.GetEventID;

            if (id != null)
            {
                var targetId = id.Value;
                
                if(targetId > EventPoints.Count) return;
                
                var point = EventPoints[targetId];
                Show(point);
            }
            else
            {
                var point = EventPoints[0];
                Show(point);
            }
        }

        private void BackEvent()
        {
            if(_currentPathPoint == null) return;
            
            var id = _currentPathPoint.GetEventID;

            if (id != null)
            {
                var targetId = id.Value - 1;

                if(targetId < 0) return;
                
                var point = EventPoints.Where(p => p.GetEventID.Value == targetId).Select(p => p).First();
                Show(point);
            }
        }
        
        private void ShowEventCountPanel(bool show)
        {
            countPanel.SetActive(show);
        }
    }
}