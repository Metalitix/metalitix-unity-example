using System.Threading.Tasks;
using Metalitix.Scripts.Logger.SubTrackers.Data;

namespace Metalitix.Scripts.Logger.SubTrackers.Metrics
{
    public abstract class Metric
    {
        public abstract MetricData GetDataFromMetric();
        public abstract Task Initialize();
        public abstract Task Proceed();
    }
}