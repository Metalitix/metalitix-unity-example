namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public enum DatePeriod
    {
        All,
        Past24Hours,
        Past7Days,
        Past30Days,
        Custom,
        None,
    }

    public static class DatePeriodConstants
    {
        public const string Day = "1";
        public const string Week = "7";
        public const string Month = "30";
        public const string All = "all";
        public const string Custom = "custom";
        public const string None = "null";
    }
}