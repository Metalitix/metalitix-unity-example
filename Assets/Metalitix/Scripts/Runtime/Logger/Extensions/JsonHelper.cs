using System;
using System.IO;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Runtime.Logger.Extensions
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings(){
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
        
        public static string ToJson<T>(T data, NullValueHandling nullValueHandling)
        {
            Settings.NullValueHandling = nullValueHandling;
            var json = JsonConvert.SerializeObject(data, Settings);
            return json;
        }
        
        public static T FromJson<T>(string json, NullValueHandling nullValueHandling)
        {
            Settings.NullValueHandling = nullValueHandling;
            var data = JsonConvert.DeserializeObject<T>(json, Settings);
            return data;
        }
        
        public static T FromJsonFile<T>(string path, NullValueHandling nullValueHandling)
        {
            if (!File.Exists(path)) throw new NullReferenceException("File doesnt exists at this path");
            
            StreamReader reader = new StreamReader(path);
            var json = reader.ReadToEnd();
            reader.Close();
            Settings.NullValueHandling = nullValueHandling;
            var data = JsonConvert.DeserializeObject<T>(json, Settings);
            return data;
        }

        public static void ToJsonFile<T>(string path, T data, NullValueHandling nullValueHandling)
        {
            var json = ToJson(data, nullValueHandling);
            File.WriteAllText(path, json);
        }
    }
}