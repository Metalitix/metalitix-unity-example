using Metalitix.Scripts.Dashboard.Core.States.DashboardStates;
using Metalitix.Scripts.Dashboard.Panels;
using Metalitix.Scripts.Logger.Core.Base;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Metalitix.Scripts.Dashboard.Base
{
    [ExecuteInEditMode]
    public class DashboardContainer : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private PreviewInteractableCanvas previewInteractableCanvasPrefab;
        [SerializeField] private InfoPanel infoPanelPrefab;
        [SerializeField] private PlayerCamera playerCameraPrefab;

        private DashboardState _currentDashboardState;
        private PlayerCamera _playerCamera;
        private PreviewInteractableCanvas _previewInteractableCanvas;
        private InfoPanel _infoPanel;
        private DashboardCamera _dashboardCamera;
        private MetalitixScene _metalitixScene;
        private Scene _targetScene;

        public PlayerCamera PlayerCamera => _playerCamera;
        public PreviewInteractableCanvas PreviewInteractableCanvas => _previewInteractableCanvas;
        public InfoPanel InfoPanel => _infoPanel;
        public DashboardCamera DashboardCamera => _dashboardCamera;
        public Scene TargetScene => _targetScene;

        public void Initialize(Scene targetScene, DashboardCamera targetCamera, MetalitixScene metalitixScene)
        {
            _dashboardCamera = targetCamera;
            _targetScene = targetScene;
            _metalitixScene = metalitixScene;
            InstantiateComponents();
        }

        private void InstantiateComponents()
        {
#if UNITY_EDITOR
            _previewInteractableCanvas = (PreviewInteractableCanvas)PrefabUtility.InstantiatePrefab(previewInteractableCanvasPrefab, _targetScene);
            _previewInteractableCanvas.Initialize(_dashboardCamera);
            _previewInteractableCanvas.SetMetalitixScene(_metalitixScene);
            _infoPanel = (InfoPanel)PrefabUtility.InstantiatePrefab(infoPanelPrefab, _targetScene);
            _infoPanel.Initialize(_dashboardCamera);
            _playerCamera = (PlayerCamera)PrefabUtility.InstantiatePrefab(playerCameraPrefab, _targetScene);
            _playerCamera.Initialize(_dashboardCamera);
            _dashboardCamera.SetObjectToFollow(_playerCamera.transform);
#endif
        }

        public void SetState(DashboardState dashboardState)
        {
            _currentDashboardState?.Exit();
            _currentDashboardState = dashboardState;
            _currentDashboardState.Enter();
        }
    }
}