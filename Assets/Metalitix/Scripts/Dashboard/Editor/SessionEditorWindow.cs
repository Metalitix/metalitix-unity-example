using System;
using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Settings;
using Metalitix.Editor.Data;
using Metalitix.Editor.Tools;
using Metalitix.Editor.Web;
using Metalitix.Scripts.Dashboard.Editor.Requests.DataGetters;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Editor
{
    public class SessionEditorWindow : EditorWindow
    {
        private SessionListGetter _sessionListGetter;
        private GlobalSettings _globalSettings;
        private MetalitixBridge _metalitixBridge;

        private int _currentSessionIndex = 0;
        private int _selectedPageIndex = int.MaxValue;
        private string[] _currentSessionHeaders;
        private CancellationTokenSource _source;

        private const int SessionWindowWidth = 300;
        private const int SessionWindowHeight = 560;

        public event Action<SessionData> OnSessionSelected;
        public event Action OnWindowClosed;

        private void OnEnable()
        {
            titleContent = new GUIContent("Sessions");
            _globalSettings = MetalitixStartUpHandler.GlobalSettings;
        }

        private void OnDisable()
        {
            OnWindowClosed?.Invoke();
        }

        public void Initialize()
        {
            maxSize = new Vector2(SessionWindowWidth, SessionWindowHeight);
            minSize = maxSize;
            Show();
        }

        public async Task LoadSessions(DataRequest request)
        {
            _metalitixBridge = new MetalitixBridge(_globalSettings.ServerUrl + MetalitixConfig.SendingEndPoint);
            _sessionListGetter = new SessionListGetter(request, _metalitixBridge);
            var sessionsData = await _sessionListGetter.GetData();
            Repaint();
            ChangePage(0);
        }

        public void Clear()
        {
            _sessionListGetter = null;
        }

        private void OnGUI()
        {
            if (_sessionListGetter == null) return;

            var selectedButtonStyle = new GUIStyle(EditorStyles.miniButtonMid)
            {
                normal =
                {
                    textColor = Color.cyan
                },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            selectedButtonStyle.normal.background = selectedButtonStyle.active.background;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<<", GUILayout.Width(30)))
            {
                _selectedPageIndex = 0;
                ChangePage(_selectedPageIndex);
            }

            if (GUILayout.Button("<", GUILayout.Width(30)))
            {
                _selectedPageIndex--;
                _selectedPageIndex = Mathf.Clamp(_selectedPageIndex, 0, _sessionListGetter.PageHeaders.Count - 1);
                ChangePage(_selectedPageIndex);
            }

            var minPagesValue = _sessionListGetter.PageHeaders.Count > 4 ? 4 : _sessionListGetter.PageHeaders.Count;
            var minValue = Mathf.Clamp(_selectedPageIndex - 2, 0, _sessionListGetter.PageHeaders.Count);
            var maxValue = Mathf.Clamp(_selectedPageIndex + 2, minPagesValue, _sessionListGetter.PageHeaders.Count);

            if (_selectedPageIndex == maxValue - 1 && _sessionListGetter.PageHeaders.Count > 4)
            {
                minValue--;
            }

            for (int i = minValue; i < maxValue; i++)
            {
                var element = i + 1;
                if (GUILayout.Button(element.ToString(),
                        i == _selectedPageIndex ? selectedButtonStyle : EditorStyles.miniButtonMid))
                {
                    ChangePage(i);
                }
            }

            if (GUILayout.Button(">", GUILayout.Width(30)))
            {
                _selectedPageIndex++;
                _selectedPageIndex = Mathf.Clamp(_selectedPageIndex, 0, _sessionListGetter.PageHeaders.Count - 1);
                ChangePage(_selectedPageIndex);
            }

            if (GUILayout.Button(">>", GUILayout.Width(30)))
            {
                _selectedPageIndex = _sessionListGetter.PageHeaders.Count - 1;
                ChangePage(_selectedPageIndex);
            }

            EditorGUILayout.EndHorizontal();

            MetalitixEditorTools.PaintSpaceWithLine();

            if (_sessionListGetter.PageHeaders.TryGetValue(_selectedPageIndex, out var sessions))
            {
                var tempIndex = GUILayout.SelectionGrid(_currentSessionIndex, sessions, 1);

                if (_currentSessionIndex != tempIndex)
                {
                    _currentSessionIndex = tempIndex;

                    SelectSession();
                }
            }

            MetalitixEditorTools.PaintSpaceWithLine();
        }

        private void ChangePage(int value)
        {
            _currentSessionIndex = 0;
            _selectedPageIndex = value;
            SelectSession();
        }

        private void SelectSession()
        {
            if (_sessionListGetter.SessionDictionary.TryGetValue(_selectedPageIndex, out var sessionsOnPage))
            {
                if (sessionsOnPage.TryGetValue(_currentSessionIndex, out var sessionData))
                {
                    OnSessionSelected?.Invoke(sessionData);
                }
            }
        }
    }
}