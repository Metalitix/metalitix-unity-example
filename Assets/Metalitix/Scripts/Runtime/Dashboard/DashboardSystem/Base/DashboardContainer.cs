using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Panels;
using Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.States.DashboardStates;
using Metalitix.Scripts.Runtime.Dashboard.Movement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Metalitix.Scripts.Runtime.Dashboard.DashboardSystem.Base
{
    [ExecuteInEditMode]
    public class DashboardContainer : MonoBehaviour
    {
        [FormerlySerializedAs("previewCanvasPrefab")]
        [Header("Prefabs")]
        [SerializeField] private PreviewInteractableCanvas previewInteractableCanvasPrefab;
        [SerializeField] private InfoPanel infoPanelPrefab;
        [SerializeField] private PlayerCamera playerCameraPrefab;

        private DashboardState _currentDashboardState;
        private PlayerCamera _playerCamera;
        private PreviewInteractableCanvas _previewInteractableCanvas;
        private InfoPanel _infoPanel;
        private DashboardCamera _dashboardCamera;
        private Scene _targetScene;

        public PlayerCamera PlayerCamera => _playerCamera;
        public PreviewInteractableCanvas PreviewInteractableCanvas => _previewInteractableCanvas;
        public InfoPanel InfoPanel => _infoPanel;
        public DashboardCamera DashboardCamera => _dashboardCamera;
        public Scene TargetScene => _targetScene;

        public void Initialize(Scene targetScene, DashboardCamera targetCamera)
        {
            _dashboardCamera = targetCamera;
            _targetScene = targetScene;
            InstantiateComponents();
        }
        
        private void InstantiateComponents()
        {
            _previewInteractableCanvas = (PreviewInteractableCanvas)PrefabUtility.InstantiatePrefab(previewInteractableCanvasPrefab, _targetScene);
            _previewInteractableCanvas.Initialize(_dashboardCamera);
            _infoPanel = (InfoPanel)PrefabUtility.InstantiatePrefab(infoPanelPrefab, _targetScene);
            _infoPanel.Initialize(_dashboardCamera);
            _playerCamera = (PlayerCamera)PrefabUtility.InstantiatePrefab(playerCameraPrefab, _targetScene);
            _playerCamera.Initialize(_dashboardCamera);
            _dashboardCamera.SetObjectToFollow(_playerCamera.transform);
        }
        
        public void SetState(DashboardState dashboardState)
        {
            _currentDashboardState?.Exit();
            _currentDashboardState = dashboardState;
            _currentDashboardState.Enter();
        }
    }
}