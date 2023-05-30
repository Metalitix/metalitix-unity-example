namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Containers
{
    public static class MetalitixConfig
    {
        public const string RecordSettingsPath = "Settings/LoggerSettings";
        public const string RateSettingsKey = "Settings/SurveySettings";
        public const string GlobalSettingsPath = "Settings/GlobalSettings";
        
        public const string DataEndPoint = "/xr-analytics";
        public const string SurveyEndPoint = "/metric-surveys";
        public const string KinesisDataStream = "/data-stream";

        public const string DevUrl = "https://metalitix-dev.aircards.io/api/v1";
        public const string StageUrl = "https://metalitix-staging.aircards.io/api/v1";
        public const string ProductionUrl = "https://app.metalitix.com/api/v1";
    }
}