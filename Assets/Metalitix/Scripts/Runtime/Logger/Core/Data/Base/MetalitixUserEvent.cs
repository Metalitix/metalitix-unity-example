using System;
using Newtonsoft.Json.Linq;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class MetalitixUserEvent 
    {
        public string eventName { get; }
        public string eventType { get; }
        
        public object target { get; private set; }
        public EventPoint[] points { get; private set; }
        
        public JObject @params { get; private set; }

        public MetalitixUserEvent(string eventName, string eventType)
        {
            @params = new JObject();
            this.eventName = eventName;
            this.eventType = eventType;
        }

        public void SetTarget(object target)
        {
            this.target = target;
        }
        
        public void SetTarget(EventPoint target)
        {
            this.target = target;
        }
        
        public void SetPoints(EventPoint[] points)
        {
            this.points = points;
        }
        
        public void AddField(string fieldName, JToken value)
        {
            if(CheckIfFieldExisted(fieldName)) return;  
            
            @params.Add(fieldName, value);
        }
     
        private bool CheckIfFieldExisted(string key)
        {
            return @params.ContainsKey(key);
        }
    }
}