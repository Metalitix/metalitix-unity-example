using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Editor.Configs;
using Metalitix.Editor.Data;
using Metalitix.Editor.Enums;
using Metalitix.Editor.HeatMapAlgorithm.Algorithms;
using Metalitix.Editor.HeatMapAlgorithm.Visualizers;
using Metalitix.Editor.Preview;
using Metalitix.Editor.Settings;
using Metalitix.Editor.Tools;
using Metalitix.Editor.Web;
using Metalitix.Scripts.Dashboard.Base;
using Metalitix.Scripts.Dashboard.Core.States.DashboardStates;
using Metalitix.Scripts.Dashboard.Editor.Requests.DataGetters;
using Metalitix.Scripts.Dashboard.Editor.Requests.DataPatchers;
using Metalitix.Scripts.Dashboard.Editor.Roles;
using Metalitix.Scripts.Preview.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SessionState = Metalitix.Scripts.Dashboard.Core.States.DashboardStates.SessionState;

namespace Metalitix.Scripts.Dashboard.Editor
{
    public class MetalitixDashboardEditor : EditorWindow
    {
        private GameObject _targetMetalitixDashboardModel;
        private AuthorizationWindow _authorizationWindow;
        private SessionEditorWindow _sessionEditorWindow;
        private HeatMapSettingsWindow _heatMapSettingsWindow;
        private MetalitixVisualizer _metalitixVisualizer;
        private MetalitixBridge _metalitixBridge;
        private ScenePreviewRenderer _previewRenderer;
        private GlobalSettings _globalSettings;
        private DashboardSettings _dashboardSettings;
        private HeatMapSettings _heatMapSettings;
        private DashboardContainer _dashboardContainer;
        private DashboardCamera _dashboardCamera;
        private Bounds _bounds;

        private bool _isInitialized;
        private bool _isLogin;
        private bool _isFilterActive;
        private bool _lastFilterState;

        [Header("Processes")]
        private RestrictedProcess _heatMapSettingsProcess;

        private string _datePeriodKey;
        private int? _selectedFilterId;
        private EnumValue<DatePeriod> _datePeriod;
        private EnumValue<RenderType> _renderType;
        private List<FilterWrapper> _filterWrappers = new List<FilterWrapper>();
        private readonly List<DataRequest> _dataRequests = new List<DataRequest>();

        private readonly string[] _dataToolbarStrings = { "HeatMap", "Session Explorer" };
        private readonly string[] _periodToolbarStrings = { "All", "Past 24 hours", "Past 7 days", "Past 30 days" };

        [Header("Custom editors")]
        private UnityEditor.Editor _viewerEditor;

        private static MetalitixDashboardEditor _editor;

        [MenuItem("Metalitix/Dashboard")]
        private static void ShowWindow()
        {
            _editor = GetWindow<MetalitixDashboardEditor>();

            var maxSize = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            _editor.maxSize = maxSize;
            _editor.position = new Rect(maxSize.x / 2, maxSize.y / 2, maxSize.x, maxSize.y);
            _editor.titleContent = new GUIContent(EditorConfig.DashboardTitle);
            _editor.ShowUtility();
        }

        private async void OnEnable()
        {
            InitializeEditors();

            EditorSceneManager.sceneClosing += EditorSceneManagerOnSceneClosing;

            _metalitixBridge = new MetalitixBridge(_globalSettings.ServerUrl);

            do
            {
                _isLogin = _dashboardSettings.ValidateAuth();
                await Task.Yield();
            }
            while (!_isLogin);

            if (string.IsNullOrEmpty(_globalSettings.APIKey))
            {
                EditorUtility.DisplayDialog(MetalitixEditorLogs.AppKeyCodeMissing, MetalitixEditorLogs.PleaseCheckAppKeyIsNotEmpty, "Ok");
                return;
            }

            var request = GetQueryRequest(12, new OrderBy(OrderByType.createdAt, true), DatePeriod.None);

            if (!await FetchProjectData(request))
            {
                EditorUtility.DisplayDialog(MetalitixEditorLogs.AppKeyNotValid, MetalitixEditorLogs.PleaseCheckAppLeyIsCorrect, "Ok");
                return;
            }

            if (request.Source.IsCancellationRequested) return;

            await FetchFiltersPresets(GetQueryRequest(10, new OrderBy(OrderByType.createdAt), DatePeriod.All));

            if (!FindDashboardModelInActiveScene()) return;
            
            _isInitialized = true;

            InitializePreviewScene();

            _datePeriod = new EnumValue<DatePeriod>(DatePeriod.All);
            _renderType = new EnumValue<RenderType>(RenderType.HeatMap);

            SetDatePeriod(_datePeriod.CurrentEnumValue);

            _datePeriod.OnValueChanged += OnDatePeriodChanged;
            _renderType.OnValueChanged += CheckRenderType;

            CreateProcesses();

            await LoadProjectHeatMap();
        }

        private void CreateProcesses()
        {
            var auth = _dashboardSettings.AuthRole;
            var workspace = _dashboardSettings.WorkspaceRole;
            var projectRole = _dashboardSettings.ProjectRole;

            _heatMapSettingsProcess = new ChangeHeatMapProcess(auth, workspace, projectRole);
        }

        private void EditorSceneManagerOnSceneClosing(Scene scene, bool removingscene)
        {
            Close();
        }

        private void InitializeEditors()
        {
            _dashboardSettings = MetalitixStartUpHandler.DashboardSettings;
            _globalSettings = MetalitixStartUpHandler.GlobalSettings;
            _heatMapSettings = MetalitixStartUpHandler.HeatMapSettings;
            _viewerEditor = MetalitixEditorTools.CreateEditor(_dashboardSettings);
        }

        private void InitializePreviewScene()
        {
            var editorSettings = MetalitixStartUpHandler.EditorSettings;
            _previewRenderer = new ScenePreviewRenderer(60, editorSettings.DashboardBackgroundColor, editorSettings.GraphicsFormatForScenePreview, position.width, position.height);

            var visualizerPath = $"{EditorConfig.ObjectKey}MetalitixVisualizer";
            _metalitixVisualizer = Instantiate(Resources.Load(visualizerPath, typeof(MetalitixVisualizer))) as MetalitixVisualizer;

            if (_metalitixVisualizer == null)
            {
                throw new NullReferenceException(MetalitixEditorLogs.VisualizerNotFound);
            }

            _previewRenderer.MoveWithoutInstance(_metalitixVisualizer.gameObject);
            MoveModelViewer();
            _dashboardCamera = _previewRenderer.RenderCamera.gameObject.AddComponent<DashboardCamera>();
            _dashboardCamera.Initialize();
            InitializeContainer();
            _metalitixVisualizer.Initialize(_targetMetalitixDashboardModel, _previewRenderer.Scene);
            _metalitixVisualizer.VisualizeWrapper();
        }

        private void InitializeContainer()
        {
            var dashboardContainerPath = $"{EditorConfig.ObjectKey}DashboardContainer";
            _dashboardContainer = Instantiate(Resources.Load(dashboardContainerPath, typeof(DashboardContainer)))
                as DashboardContainer;
            _dashboardContainer.Initialize(_previewRenderer.Scene, _dashboardCamera);
            _previewRenderer.MoveWithoutInstance(_dashboardContainer.gameObject);
        }

        private void MoveModelViewer()
        {
            var instanceVisualizer = _previewRenderer.MoveToTheSceneWithInstance(_targetMetalitixDashboardModel);
            instanceVisualizer.transform.parent = _metalitixVisualizer.GetWrapperTransform();
        }

        private bool FindDashboardModelInActiveScene()
        {
            var metalitixDashboardModel = FindObjectOfType<MetalitixDashboardModel>();

            if (metalitixDashboardModel == null)
            {
                MetalitixDebug.LogError(this, MetalitixEditorLogs.NoFoundDashboardModelInScene);
                return false;
            }

            _targetMetalitixDashboardModel = metalitixDashboardModel.gameObject;
            return true;
        }
            
        private async Task FetchFiltersPresets(DataRequest request)
        {
            var filterGetter = new FiltersGetter(request, _metalitixBridge);
            await filterGetter.GetData();
            _filterWrappers = filterGetter.GetFilterWrappers();
        }

        private async Task<bool> FetchProjectData(DataRequest request)
        {
            var dataGetter = new TargetProjectGetter(_globalSettings.APIKey, request, _metalitixBridge);
            var data = await dataGetter.GetData();

            if (request.Source.IsCancellationRequested) return true;
            if (dataGetter.MetalitixProjectData == null) return false;

            _dashboardSettings.SetTargetProject(dataGetter.MetalitixProjectData);
            _dashboardSettings.SetProjectMember(dataGetter.ProjectMember);
            _dashboardSettings.SetWorkspaceMember(dataGetter.WorkspaceMember);
            _heatMapSettings.SyncData(_dashboardSettings.TargetProject.settings);
            _heatMapSettings.UpdateValues();
            return true;
        }

        private void OnDisable()
        {
            Cancel();
            _metalitixVisualizer = null;
            _metalitixBridge = null;
            _filterWrappers.Clear();

            if (_sessionEditorWindow != null)
            {
                CloseSessionsWindow();
            }

            if (_heatMapSettingsWindow != null)
            {
                _heatMapSettingsWindow.Close();
            }

            AssetDatabase.SaveAssetIfDirty(_viewerEditor);
            DisposeCurrentPreviewScene();
            EditorSceneManager.sceneClosing -= EditorSceneManagerOnSceneClosing;
        }

        private void DisposeCurrentPreviewScene()
        {
            if (_previewRenderer == null) return;

            _previewRenderer.Dispose();
            _previewRenderer = null;
        }

        private void OnGUI()
        {
            if (!_isLogin)
            {
                DrawLoginPage();
                return;
            }

            if (!_isInitialized) return;

            DrawViewerButtons();
            var sizeOfTextureWindow = DrawViewerTexture();

            _previewRenderer.DoRaycast(sizeOfTextureWindow.x, sizeOfTextureWindow.y);

            if (_sessionEditorWindow == null)
            {
                DrawHeatMapSettingsButton();
            }

            EditorUtility.SetDirty(_dashboardSettings);
        }

        private void DrawLoginPage()
        {
            float labelWidth = 800f;
            float labelHeight = 40f;
            Rect labelRect = new Rect(position.width / 2 - labelWidth / 2, position.height / 2 - labelHeight / 2, labelWidth,
                labelHeight);
            GUIContent labelContent = new GUIContent(MetalitixEditorLogs.PleaseLogin);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 25,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(labelRect, labelContent, labelStyle);

            float buttonWidth = 120f;
            float buttonHeight = 30f;
            Rect buttonRect = new Rect(position.width / 2 - buttonWidth / 2, position.height / 2 + labelHeight, buttonWidth,
                buttonHeight);

            GUIContent buttonContent = new GUIContent("Login");

            if (GUI.Button(buttonRect, buttonContent))
            {
                Login();
            }
        }

        private void Login()
        {
            _authorizationWindow = CreateInstance<AuthorizationWindow>();
            _authorizationWindow.Initialize(new Vector2(position.x + position.width / 2, position.y + position.height / 2));
            _authorizationWindow.Show();
        }

        private void DrawRefreshButton()
        {
            var content = new GUIContent("", MetalitixStartUpHandler.RefreshTexture);

            if (GUI.Button(new Rect(position.width - 36, 0, 35, 35), content))
            {
                var renderTypeValue = _renderType.CurrentEnumValue;

                switch (renderTypeValue)
                {
                    case RenderType.HeatMap:
                        LoadProjectHeatMap();
                        break;
                    case RenderType.SessionExplorer:
                        CreateSessionWindow();
                        var request = GetQueryRequest(20, new OrderBy(OrderByType.startDate, true), _datePeriod.CurrentEnumValue, _selectedFilterId);
                        _sessionEditorWindow.LoadSessions(request);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void DrawHeatMapSettingsButton()
        {
            if (!_heatMapSettingsProcess.IsValid) return;

            var content = new GUIContent("", MetalitixStartUpHandler.SettingsTexture);

            if (GUI.Button(new Rect(5, 35, 35, 35), content))
            {
                if (_heatMapSettingsWindow == null)
                {
                    Vector2 mousePos = GUIUtility.GUIToScreenPoint(new Vector2(5, 35));
                    _heatMapSettingsWindow = CreateInstance<HeatMapSettingsWindow>();
                    _heatMapSettingsWindow.Initialize(mousePos);
                    _heatMapSettingsWindow.OnApply += OnApply;
                    return;
                }

                CloseHeatMapSettingWindow();
            }
        }

        private void DrawViewerButtons()
        {
            _renderType.CurrentValue = GUI.Toolbar(new Rect(0, 0, 250, 30), _renderType.CurrentValue, _dataToolbarStrings);
            _datePeriod.CurrentValue = GUI.Toolbar(new Rect(position.width - 656, 0, 500, 30), _datePeriod.CurrentValue, _periodToolbarStrings);

            var isDisabled = _filterWrappers.Count == 0;

            EditorGUI.BeginDisabledGroup(isDisabled);
            DrawDropdown(new Rect(position.width - 156, 0, 125, 30), new GUIContent("Filters"));
            EditorGUI.EndDisabledGroup();

            DrawRefreshButton();
        }

        private void DrawDropdown(Rect buttonPosition, GUIContent label)
        {
            _isFilterActive = GUI.Toggle(buttonPosition, _isFilterActive, label, "Button");

            if (_isFilterActive != _lastFilterState)
            {
                if (_selectedFilterId != null)
                    _isFilterActive = _lastFilterState;
                else
                    _lastFilterState = _isFilterActive;

                if (_isFilterActive)
                {
                    _datePeriod.CurrentEnumValue = DatePeriod.None;
                    GenericMenu menu = new GenericMenu();

                    foreach (var filterWrapper in _filterWrappers)
                    {
                        menu.AddItem(new GUIContent(filterWrapper.filterName), filterWrapper.isSelected,
                            OnFilterSelected,
                            filterWrapper);
                    }

                    menu.DropDown(buttonPosition);
                }
                else
                {
                    DeactivateFilters();
                }
            }
        }

        private void CreateSessionWindow()
        {
            CloseHeatMapSettingWindow();

            if (_sessionEditorWindow != null)
            {
                _sessionEditorWindow.Close();
                _sessionEditorWindow = null;
            }

            _sessionEditorWindow = CreateInstance<SessionEditorWindow>();
            _sessionEditorWindow.OnSessionSelected += LoadSessionRecords;
            _sessionEditorWindow.Initialize();

            this.Dock(_sessionEditorWindow, Docker.DockPosition.Left);
        }

        private void CloseHeatMapSettingWindow()
        {
            if (_heatMapSettingsWindow == null) return;

            _heatMapSettingsWindow.OnApply -= OnApply;
            _heatMapSettingsWindow.Close();
            _heatMapSettingsWindow = null;
        }

        private void OnFilterSelected(object data)
        {
            var filterWrapper = (FilterWrapper)data;

            if (filterWrapper.isSelected)
                filterWrapper.UnSelect();
            else
                filterWrapper.Select();

            if (filterWrapper.id == _selectedFilterId)
            {
                filterWrapper.UnSelect();
                DeactivateFilters();
                return;
            }

            foreach (var wrapper in _filterWrappers.Where(wrapper => wrapper.id != filterWrapper.id && wrapper.isSelected))
                wrapper.UnSelect();

            if (_filterWrappers.All(w => !w.isSelected))
                _selectedFilterId = null;
            else
                _selectedFilterId = filterWrapper.id;

            CheckRenderType(_renderType.CurrentEnumValue);
        }

        private async void CheckRenderType(RenderType renderType)
        {
            CloseSessionsWindow();

            switch (renderType)
            {
                case RenderType.HeatMap:
                    await LoadProjectHeatMap();
                    break;
                case RenderType.SessionExplorer:
                    CreateSessionWindow();
                    var request = GetQueryRequest(20, new OrderBy(OrderByType.startDate, true), _datePeriod.CurrentEnumValue, _selectedFilterId);
                    await _sessionEditorWindow.LoadSessions(request);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderType), renderType, null);
            }
        }

        private void OnDatePeriodChanged(DatePeriod datePeriod)
        {
            if (datePeriod != DatePeriod.None)
            {
                DeactivateFilters();
                CheckRenderType(_renderType.CurrentEnumValue);
            }

            SetDatePeriod(datePeriod);
        }

        private void SetDatePeriod(DatePeriod datePeriod)
        {
            _datePeriodKey = datePeriod switch
            {
                DatePeriod.All => DatePeriodConstants.All,
                DatePeriod.Past24Hours => DatePeriodConstants.Day,
                DatePeriod.Past7Days => DatePeriodConstants.Week,
                DatePeriod.Past30Days => DatePeriodConstants.Month,
                DatePeriod.Custom => DatePeriodConstants.Custom,
                DatePeriod.None => null,
                _ => throw new ArgumentOutOfRangeException(nameof(datePeriod), datePeriod, null)
            };
        }

        private void Update()
        {
            Repaint();

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _editor.Close();
            }
        }

        private Vector2 DrawViewerTexture()
        {
            if (_previewRenderer == null) return Vector2.zero;

            _previewRenderer.Render();
            var size = _previewRenderer.GetGUIPreviewSize();
            var rect = new Rect(0, 30, size.x, size.y);
            EditorGUI.DrawPreviewTexture(rect, _previewRenderer.Texture2D);
            return size;
        }

        private async void LoadSessionRecords(SessionData sessionData)
        {
            var sessionState = new EmptyState(_dashboardContainer);
            _dashboardContainer.SetState(sessionState);

            Cancel();
            var request = GetQueryRequest(500, new OrderBy(OrderByType.timestamp), _datePeriod.CurrentEnumValue);
            var dataGetter = new SessionDataGetter(sessionData.sessionId, request, _metalitixBridge);
            var records = await dataGetter.GetData();

            if (records != null && !request.Source.IsCancellationRequested)
            {
                await DrawPaths(sessionData, records);
            }
        }

        private async Task LoadProjectHeatMap()
        {
            Cancel();

            var sessionState = new EmptyState(_dashboardContainer);
            _dashboardContainer.SetState(sessionState);

            var request = _selectedFilterId == null ?
                GetQueryRequest(500, new OrderBy(OrderByType.timestamp), _datePeriod.CurrentEnumValue) :
                GetQueryRequest(500, new OrderBy(OrderByType.timestamp), _datePeriod.CurrentEnumValue, _selectedFilterId);

            var dataGetter = new HeatMapDataGetter(request, _metalitixBridge);
            dataGetter.OnBigDataReceived += BackendCalculation;
            var records = await dataGetter.GetData();
            dataGetter.OnBigDataReceived -= BackendCalculation;

            if (records.Count != 0 && !request.Source.IsCancellationRequested)
            {
                await DrawHeatMap(records);
            }
        }

        private async void OnApply()
        {
            await PatchHeatMap();
            await LoadProjectHeatMap();
        }

        private async Task PatchHeatMap()
        {
            var request = GetRequest();
            var heatMapSettingsData = new HeatMapSettingsData(_heatMapSettings.CameraSize,
                _heatMapSettings.GazeScale, _heatMapSettings.GazeIntensity,
                _heatMapSettings.PositionScale, _heatMapSettings.PositionIntensity);

            var heatMapPatcher = new HeatMapPatcher(_dashboardSettings.ProjectID, heatMapSettingsData, request,
                _metalitixBridge);

            await heatMapPatcher.UpdateData();
        }

        private async void BackendCalculation()
        {
            Cancel();
            var request = GetRequest();
            var calculationGetter = new BackendCalculationGetter(_selectedFilterId, _datePeriodKey, request, _metalitixBridge);
            var data = await calculationGetter.GetData();

            if (data.gazeHeatMap.Count == 0 && data.positionHeatMap.Count == 0)
            {
                EditorUtility.DisplayDialog("Warning", MetalitixEditorLogs.HeatMapNotCalculatedYetForPeriod + _datePeriodKey, "Ok");
                return;
            }

            await PreloadDraw(new HeatMapAlgorithm(), data.gazeHeatMap);
            await PreloadDraw(new PointDrawerAlgorithm(), data.positionHeatMap);
        }

        private async Task PreloadDraw(DrawerAlgorithm drawerAlgorithm, List<Vector4> data)
        {
            if (data.Count != 0)
            {
                await _metalitixVisualizer.SetData(_heatMapSettings.CameraSize, null, new CancellationTokenSource(), drawerAlgorithm);
                await drawerAlgorithm.PreloadDraw(data);
            }
        }

        private DataRequest GetRequest()
        {
            var source = new CancellationTokenSource();
            var projectData = _dashboardSettings.TargetProject;
            var request = new DataRequest(_dashboardSettings.UserID, _dashboardSettings.AuthToken, projectData, source);
            _dataRequests.Add(request);
            return request;
        }

        private DataRequest GetQueryRequest(int limit, OrderBy orderBy, DatePeriod datePeriod, int? filterID = null)
        {
            var request = GetRequest();
            var query = filterID == null ?
                new PageQuery(1, limit, orderBy, datePeriod) :
                new PageQuery(1, limit, orderBy, filterID.Value);
            request.AddQuery(query);
            return request;
        }

        private async Task DrawPaths(SessionData sessionData, List<Record> loadedRecords)
        {
            var source = new CancellationTokenSource();
            var pathDrawer = new PathDrawerAlgorithm();

            pathDrawer.OnPathReady += (pathPoints, eventPoints) => OnPathReady(pathPoints, eventPoints, sessionData);

            await _metalitixVisualizer.SetData(_heatMapSettings.CameraSize, loadedRecords, source, pathDrawer);
            await _metalitixVisualizer.DrawBySessionID(sessionData.sessionId);
        }

        private void OnPathReady(List<PathPoint> pathPoints, List<PathPoint> eventPoints, SessionData sessionData)
        {
            var sessionState = new SessionState(pathPoints, eventPoints, sessionData, _dashboardContainer);
            _dashboardContainer.SetState(sessionState);
        }

        private async Task DrawHeatMap(List<Record> loadedRecords)
        {
            var source = new CancellationTokenSource();
            var pointDrawer = new PointDrawerAlgorithm();
            var heatDrawer = new HeatMapAlgorithm();

            await _metalitixVisualizer.SetData(_heatMapSettings.CameraSize, loadedRecords, source, pointDrawer, heatDrawer);
            await _metalitixVisualizer.Draw();
        }

        private void CloseSessionsWindow()
        {
            if (_sessionEditorWindow == null) return;

            _sessionEditorWindow.OnSessionSelected -= LoadSessionRecords;
            _sessionEditorWindow.Clear();
            _sessionEditorWindow.Close();
        }

        private void DeactivateFilters()
        {
            _isFilterActive = false;
            _selectedFilterId = null;

            foreach (var wrapper in _filterWrappers)
            {
                wrapper.UnSelect();
            }
        }

        private async void Cancel()
        {
            foreach (var dataRequest in _dataRequests)
            {
                dataRequest.CloseRequest();
            }

            _dataRequests.Clear();
            EditorUtility.ClearProgressBar();

            if (_metalitixVisualizer == null) return;

            await _metalitixVisualizer.DeleteVisual();
        }
    }
}