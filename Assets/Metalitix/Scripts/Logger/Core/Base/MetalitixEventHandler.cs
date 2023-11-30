using System;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Enums;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class MetalitixEventHandler
    {
        private Action<MetalitixUserEvent> onEventCreate;

        public MetalitixEventHandler(Action<MetalitixUserEvent> onEventCreate)
        {
            this.onEventCreate = onEventCreate;
        }
        
        public void LogCustomEvent(string eventName, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.Custom;
            var @event = new MetalitixUserEvent(eventName, eventType);

            foreach (var tuple in @params)
            {
                @event.AddField(tuple.key, tuple.value);
            }

            onEventCreate?.Invoke(@event);
        }

        public void LogKeyDownEvent(string keyName, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.KeyDown;
            var eventName = MetalitixUserEventNames.KeyDown;
            var state = PointStates.Pressed;
            LogPredefinedEvent(eventName, eventType, null, @params);
        }

        public void LogKeyPressEvent(string keyName, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.KeyPress;
            var eventName = MetalitixUserEventNames.KeyPress;
            LogPredefinedEvent(eventName, eventType, null, @params);
        }

        public void LogKeyUpEvent(string keyName, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.KeyUp;
            var eventName = MetalitixUserEventNames.KeyUp;
            LogPredefinedEvent(eventName, eventType, null, @params);
        }

        public void LogMouseEnterEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseEnter;
            var eventName = MetalitixUserEventNames.MouseEnter;
            var state = PointStates.Stationary;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseLeaveEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseLeave;
            var eventName = MetalitixUserEventNames.MouseLeave;
            var state = PointStates.Stationary;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseOverEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseOver;
            var eventName = MetalitixUserEventNames.MouseOver;
            var state = PointStates.Stationary;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseOutEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseOut;
            var eventName = MetalitixUserEventNames.MouseOut;
            var state = PointStates.Stationary;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseDownEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseDown;
            var eventName = MetalitixUserEventNames.MouseDown;
            var state = PointStates.Pressed;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseUpEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseUp;
            var eventName = MetalitixUserEventNames.MouseUp;
            var state = PointStates.Released;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMouseMoveEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MouseMove;
            var eventName = MetalitixUserEventNames.MouseMove;
            var state = PointStates.Updated;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogMousePressEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.MousePress;
            var eventName = MetalitixUserEventNames.MousePress;
            var state = PointStates.Stationary;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogTouchTapEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.TouchTap;
            var eventName = MetalitixUserEventNames.TouchTap;
            var state = PointStates.Pressed;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogTouchStartEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.TouchStart;
            var eventName = MetalitixUserEventNames.TouchStart;
            var state = PointStates.Pressed;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogTouchMoveEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.TouchMove;
            var eventName = MetalitixUserEventNames.TouchMove;
            var state = PointStates.Updated;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }

        public void LogTouchEndEvent(float x, float y, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.TouchEnd;
            var eventName = MetalitixUserEventNames.TouchEnd;
            var state = PointStates.Released;
            
            var target = new EventPoint(state, DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                new Vector2(x, y));
            
            LogScreenPredefinedEvent(eventName, eventType, target, @params);
        }
    
        public void LogZoomStartEvent(float zoomValue, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.ZoomStart;
            var eventName = MetalitixUserEventNames.ZoomStart;
            LogPredefinedEvent(eventName, eventType, zoomValue, @params);
        }

        public void LogZoomUpdateEvent(float zoomValue, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.ZoomUpdate;
            var eventName = MetalitixUserEventNames.ZoomUpdate;
            LogPredefinedEvent(eventName, eventType, zoomValue, @params);
        }

        public void LogZoomEndEvent(float zoomValue, params (string key, JToken value)[] @params)
        {
            var eventType = MetalitixUserEventType.ZoomEnd;
            var eventName = MetalitixUserEventNames.ZoomEnd;
            LogPredefinedEvent(eventName, eventType, zoomValue, @params);
        }

        private void LogPredefinedEvent(string name, string type, object target, (string key, JToken value)[] @params)
        {
            var @event = new MetalitixUserEvent(name, type);
            
            @event.SetTarget(target);

            foreach (var tuple in @params)
            {
                @event.AddField(tuple.key, tuple.value);
            }

            onEventCreate?.Invoke(@event);
        }
        
        private void LogScreenPredefinedEvent(string name, string type, EventPoint target, (string key, JToken value)[] @params)
        {
            var @event = new MetalitixUserEvent(name, type);
            
            @event.SetTarget(target);

            foreach (var tuple in @params)
            {
                @event.AddField(tuple.key, tuple.value);
            }

            onEventCreate?.Invoke(@event);
        }
    }
}