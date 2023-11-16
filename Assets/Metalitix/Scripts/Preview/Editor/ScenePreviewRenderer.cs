using System;
using System.Collections.Generic;
using System.Linq;
using Metalitix.Core.Base;
using Metalitix.Core.Data.Containers;
using Metalitix.Editor.Data;
using Metalitix.Editor.Preview;
using Metalitix.Editor.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Metalitix.Scripts.Preview.Editor
{
    public class ScenePreviewRenderer
    {
        private Camera _renderCamera = null;
        private RenderTexture _renderTexture;
        private EventSystem _eventSystem;
        private Texture2D _texture2D;
        private Scene _scene;

        private SupportedAspects _aspectChoiceIdx = SupportedAspects.Aspect16by9;
        private float _curAspect;

        private readonly float _worldScreenHeight = 5;
        private readonly int _renderTextureHeight = 1080;
        private readonly string[] _layersToInteract;

        public Scene Scene => _scene;
        public Texture2D Texture2D => _texture2D;
        public Camera RenderCamera => _renderCamera;

        private IScenePreviewPointerMove _lastHighlightedByMouse;
        private Dictionary<Type, GameObject> _gameObjects;
        private readonly Dictionary<Vector2, IScenePreviewInteractable> _interactableDataCache;

        private const float UVRangeToFindInCache = 0.006f;

        public ScenePreviewRenderer(float fov, Color backgroundColor, GraphicsFormat graphicsFormat, float width, float height)
        {
            _gameObjects = new Dictionary<Type, GameObject>();
            _interactableDataCache = new Dictionary<Vector2, IScenePreviewInteractable>();
            _scene = EditorSceneManager.NewPreviewScene();
            _layersToInteract = new[] { MetalitixStartUpHandler.PreviewInteractableLayer, MetalitixStartUpHandler.PreviewLayer };

            if (!_scene.IsValid())
                throw new InvalidOperationException(MetalitixEditorLogs.PreviewSceneError);

            InitializeScene(fov, backgroundColor, graphicsFormat);
        }

        private void InitializeScene(float fov, Color backgroundColor, GraphicsFormat graphicsFormat)
        {
            var camera = SpawnGameObject("Camera", typeof(Camera));
            _renderCamera = camera.GetComponent<Camera>();

            _curAspect = AspectToFloat(_aspectChoiceIdx);
            _renderCamera.cameraType = CameraType.SceneView;
            _renderCamera.clearFlags = CameraClearFlags.SolidColor;
            _renderCamera.backgroundColor = backgroundColor;
            _renderCamera.enabled = true;

            float inverseFov = Mathf.InverseLerp(0, 100, fov);
            float normalFov = Mathf.Lerp(0, 100, inverseFov);

            _renderCamera.fieldOfView = normalFov;
            _renderCamera.nearClipPlane = 0.1f;
            _renderCamera.farClipPlane = 10000f;
            _renderCamera.renderingPath = RenderingPath.Forward;
            _renderCamera.useOcclusionCulling = false;
            _renderCamera.scene = _scene;
            _renderCamera.aspect = _curAspect;
            _renderCamera.orthographicSize = _renderTextureHeight / 2f;
            _renderTexture = new RenderTexture(Mathf.RoundToInt(_curAspect * _renderTextureHeight),
                _renderTextureHeight, 32, graphicsFormat);
            _renderCamera.targetTexture = _renderTexture;
            _texture2D = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false);
            _texture2D.Apply(false);

            _renderCamera.transform.position = Vector3.forward * -10f;

            SpawnGameObject("EventSystem", typeof(EventSystem));
            _eventSystem = FindGameObject<EventSystem>();
        }

        public void DoRaycast(float windowWidth, float windowHeight)
        {
            if (_layersToInteract.Length == 0) return;

            Event currentEvent = Event.current;

            ExecuteHighlightable(windowWidth, windowHeight, currentEvent);

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                ExecuteInteractable(windowWidth, windowHeight, currentEvent);
            }
        }

        private void ExecuteInteractable(float windowWidth, float windowHeight, Event currentEvent)
        {
            var interactableData =
                ExecuteRaycast<IScenePreviewInteractable>(currentEvent, windowWidth, windowHeight);

            if (interactableData.interactable != default)
            {
                if (!_interactableDataCache.ContainsKey(interactableData.interactableData.uv))
                    _interactableDataCache.Add(interactableData.interactableData.uv, interactableData.interactable);

                interactableData.interactable.Interact();
            }
            else
            {
                var uv = interactableData.interactableData.uv;
                var key = _interactableDataCache.Keys.FirstOrDefault(k =>
                    k.x >= uv.x - UVRangeToFindInCache && k.x <= uv.x + UVRangeToFindInCache &&
                    k.y >= uv.y - UVRangeToFindInCache && k.y <= uv.y + UVRangeToFindInCache);

                if (_interactableDataCache.TryGetValue(key, out var value))
                {
                    value.Interact();
                }
            }

            var interactableUV =
                ExecuteRaycast<IScenePreviewInteractableData>(currentEvent, windowWidth, windowHeight);
            interactableUV.interactable?.Interact(interactableUV.interactableData);
        }

        private void ExecuteHighlightable(float windowWidth, float windowHeight, Event currentEvent)
        {
            var highlightableData = ExecuteRaycast<IScenePreviewPointerMove>(currentEvent, windowWidth, windowHeight);

            if (highlightableData.interactable != null)
            {
                if (highlightableData.interactable != _lastHighlightedByMouse)
                {
                    _lastHighlightedByMouse = highlightableData.interactable;
                    _lastHighlightedByMouse?.PointerEnter();
                }

                _lastHighlightedByMouse?.PointerEnter();
            }
            else
            {
                _lastHighlightedByMouse?.PointerExit();
            }
        }

        private (T interactable, InteractableData interactableData) ExecuteRaycast<T>(Event currentEvent, float windowWidth, float windowHeight)
        {
            var mousePos = currentEvent.mousePosition;
            var calculatedMousePosY = mousePos.y - 33.5f;
            var uvCoordinates = new Vector2(mousePos.x / windowWidth, 1 - calculatedMousePosY / windowHeight);
            var ray = _renderCamera.ViewportPointToRay(uvCoordinates);
            var depth = GetDepthValue(uvCoordinates);
            var distance = depth * _renderCamera.farClipPlane;
            var newDirection = ray.direction.normalized * distance;
            var cullingMask = _renderCamera.cullingMask;
            var interactableLayers = new HashSet<string>();

            for (var i = 0; i < 32; i++)
            {
                if ((cullingMask & 1 << i) == 0) continue;
                var layerName = LayerMask.LayerToName(i);

                if (_layersToInteract.Any(layer => layer.Equals(layerName)))
                {
                    interactableLayers.Add(layerName);
                }
            }

            InteractableData interactableData =
                new InteractableData(uvCoordinates, mousePos, windowWidth, windowHeight);

            if (_scene.GetPhysicsScene().Raycast(ray.origin, newDirection, out var hitInfo, Mathf.Infinity,
                    LayerMask.GetMask(interactableLayers.ToArray())))
            {
                var targetObject = hitInfo.collider.gameObject;

                if (targetObject.TryGetComponent<T>(out var interactable))
                {
                    return (interactable, interactableData);
                }
            }

            return (default, interactableData);
        }

        public T FindGameObject<T>()
        {
            var gameObjects = _scene.GetRootGameObjects();

            if (gameObjects.Length == 0) return default;

            foreach (var obj in gameObjects)
            {
                if (obj.TryGetComponent<T>(out var target))
                    return target;
            }

            return default;
        }

        public GameObject SpawnGameObject(string name, params Type[] components)
        {
            var go = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.DontSave, components);

            foreach (var component in components)
            {
                _gameObjects.Add(component, go);
            }

            SceneManager.MoveGameObjectToScene(go, _scene);
            return go;
        }

        public void InstantiatePrefab<T>(T go) where T : Object
        {
            PrefabUtility.InstantiatePrefab(go, _scene);
        }

        public GameObject MoveToTheSceneWithInstance(GameObject go)
        {
            var instance = Object.Instantiate(go, go.transform.position, go.transform.rotation);
            SceneManager.MoveGameObjectToScene(instance, _scene);
            return instance;
        }

        public void MoveWithoutInstance(GameObject go)
        {
            SceneManager.MoveGameObjectToScene(go, _scene);
        }

        public void Render()
        {
            if (_renderTexture == null) return;

            _renderCamera.Render();
            Graphics.CopyTexture(_renderTexture, _texture2D);
        }

        public void Dispose()
        {
            if (_renderCamera != null && _renderCamera.targetTexture != null)
            {
                _renderCamera.targetTexture.Release();
            }

            _renderTexture = null;
            _texture2D = null;
            Object.DestroyImmediate(_renderCamera);
            EditorSceneManager.ClosePreviewScene(_scene);
        }

        public Vector2 GetGUIPreviewSize()
        {
            var camSizeWorld = new Vector2(_renderTexture.width, _renderTexture.height);
            var scaleFactor = EditorGUIUtility.currentViewWidth / camSizeWorld.x;
            var size = new Vector2(EditorGUIUtility.currentViewWidth, scaleFactor * camSizeWorld.y);
            return size;
        }

        private float GetDepthValue(Vector2 uv)
        {
            var temTexture = _texture2D;
            Texture2D depthTexture2D = new Texture2D(temTexture.width, temTexture.height, TextureFormat.RFloat, false);
            RenderTexture.active = _renderCamera.activeTexture;
            depthTexture2D.ReadPixels(new Rect(0, 0, temTexture.width, temTexture.height), 0, 0);
            depthTexture2D.Apply();
            RenderTexture.active = null;

            var depth = depthTexture2D.GetPixel(Mathf.FloorToInt(uv.x * temTexture.width), Mathf.FloorToInt(uv.y * temTexture.height)).b;
            return depth;
        }

        private float AspectToFloat(SupportedAspects aspects)
        {
            switch (aspects)
            {
                case SupportedAspects.Aspect16by10:
                    return 16 / 10f;
                case SupportedAspects.Aspect16by9:
                    return 16 / 9f;
                case SupportedAspects.Aspect4by3:
                    return 4 / 3f;
                case SupportedAspects.Aspect5by4:
                    return 5 / 4f;
                default:
                    throw new ArgumentException();
            }
        }

        private enum SupportedAspects
        {
            Aspect4by3 = 1,
            Aspect5by4 = 2,
            Aspect16by10 = 3,
            Aspect16by9 = 4
        };
    }
}