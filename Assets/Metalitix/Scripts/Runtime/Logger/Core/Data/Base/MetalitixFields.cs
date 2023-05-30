using Newtonsoft.Json.Linq;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    public sealed class MetalitixFields
    {
        private JObject customFields;

        public MetalitixFields()
        {
            customFields = new JObject();
        }

        public JObject GetFields()
        {
            return customFields;
        }
        
        public void ClearFields()
        {
            customFields = new JObject();
        }

        public void SetFields(JObject dictionary)
        {
            customFields = dictionary;
        }

        public void RemoveField(string fieldName)
        {
            if(!CheckExisted(fieldName)) return;  
            
            customFields.Remove(fieldName);
        }
        
        public void AddField(string fieldName, JToken value)
        {
            if(CheckExisted(fieldName)) return;  
            
            customFields.Add(fieldName, value);
        }

        private bool CheckExisted(string key)
        {
            return customFields.ContainsKey(key);
        }
    }
}