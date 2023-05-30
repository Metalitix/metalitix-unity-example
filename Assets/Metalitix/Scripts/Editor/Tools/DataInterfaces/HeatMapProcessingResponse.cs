using System;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class HeatMapProcessingResponse
    {
        public string id { get; set; }
        public string projectId { get; set; }
        public string timestamp { get; set; }
        public string status { get; set; }
        public string errors { get; set; }
        public string coverImage { get; set; }
        public string uploadUrl { get; set; }
        public string location { get; set; }
        public string filterSetId { get; set; }
        public string heatmapType { get; set; }
        public string timespan { get; set; }
        public bool emailSending { get; set; }
        public string instanceId { get; set; }
        
        public HeatMapProcessingResponse(string id, string projectId, string timestamp, string status, string errors, 
            string coverImage, string uploadUrl, string location, string filterSetId, string heatmapType, string timespan,
            bool emailSending, string instanceId)
        {
            this.id = id;
            this.projectId = projectId;
            this.timestamp = timestamp;
            this.status = status;
            this.errors = errors;
            this.coverImage = coverImage;
            this.uploadUrl = uploadUrl;
            this.location = location;
            this.filterSetId = filterSetId;
            this.heatmapType = heatmapType;
            this.timespan = timespan;
            this.emailSending = emailSending;
            this.instanceId = instanceId;
        }
    }
}