using System;
using System.Threading;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools;
using Metalitix.Editor.Configs;
using Metalitix.Editor.Data;
using Metalitix.Editor.Settings;
using Metalitix.Editor.Tools;
using Metalitix.Editor.Web;
using UnityEditor;
using UnityEngine;

namespace Metalitix.Scripts.Dashboard.Editor
{
    public class AuthorizationWindow : EditorWindow
    {
        private DashboardSettings _dashboardSettings;
        private GlobalSettings _globalSettings;
        private MetalitixBridge _metalitixBridge;

        private const int ViewerWindowWidth = 400;
        private const int ViewerWindowHeight = 180;

        private void OnEnable()
        {
            titleContent = new GUIContent("Authorization");

            _dashboardSettings = MetalitixStartUpHandler.DashboardSettings;
            _globalSettings = MetalitixStartUpHandler.GlobalSettings;
            _metalitixBridge = new MetalitixBridge(_globalSettings.ServerUrl);
        }

        public void Initialize(Vector2 center)
        {
            maxSize = new Vector2(ViewerWindowWidth, ViewerWindowHeight);
            minSize = maxSize;
            position.Set(center.x, center.y, position.width, position.height);
        }

        private void OnGUI()
        {
            DrawAuthorizationPage();
        }

        private void DrawAuthorizationPage()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueForLogo);
            EditorGUILayout.LabelField("Authorization", MetalitixEditorTools.GetHeaderTextStyle());
            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueForLogo);

            _dashboardSettings.Login = EditorGUILayout.TextField("Email", _dashboardSettings.Login);
            _dashboardSettings.Password = EditorGUILayout.PasswordField("Password", _dashboardSettings.Password);

            EditorGUILayout.Space(MetalitixEditorTools.SpaceValueBeforeButtons);

            MetalitixEditorTools.PaintButton("Login", Login);
            EditorGUILayout.EndVertical();
        }

        private async void Login()
        {
            var authorizationData = new MetalitixAuthorizationData(_dashboardSettings.Login, _dashboardSettings.Password);

            try
            {
                var response = await _metalitixBridge.Login(authorizationData, new CancellationToken());

                if (response.email == null) throw new Exception(MetalitixEditorLogs.InvalidCredentials);

                _dashboardSettings.SetAuthResponse(response);
                _dashboardSettings.SetToken(response.token);
                EditorPrefs.SetString(EditorConfig.AuthEditorSave, response.token);
            }
            catch (Exception e)
            {
                MetalitixDebug.LogError(this, e.Message);
                EditorUtility.DisplayDialog(MetalitixEditorLogs.InvalidCredentials, MetalitixEditorLogs.PleaseCheckYourCredentials, "Ok");
            }
            finally
            {
                Close();
            }
        }
    }
}