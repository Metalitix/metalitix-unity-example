using System;
using Unity.Plastic.Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class MetalitixProjectData
    {
        public int id { get; private set; }
        public string title { get; private set; }
        public string description { get; private set; }
        public string coverImageId { get; private set; }
        public string coverImageUrl { get; private set; }
        public string scriptApiKey { get; private set; }
        public string platformType { get; private set; }
        public int ownerId { get; private set; }
        public int workspaceId { get; private set; }
        public string createdAt { get; private set; }
        public string updatedAt { get; private set; }
        public HeatMapSettingsData settings { get; }

        [JsonConstructor]
        public MetalitixProjectData(int id, string title, string description, string coverImageId, string coverImageUrl, string scriptApiKey, 
            string platformType, int ownerId, int workspaceId, string createdAt, string updatedAt, HeatMapSettingsData settings)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.coverImageId = coverImageId;
            this.coverImageUrl = coverImageUrl;
            this.scriptApiKey = scriptApiKey;
            this.platformType = platformType;
            this.ownerId = ownerId;
            this.workspaceId = workspaceId;
            this.createdAt = createdAt;
            this.updatedAt = updatedAt;
            this.settings = settings;
        }
    }
}