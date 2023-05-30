using System.Threading.Tasks;
using Metalitix.Scripts.Runtime.Logger.SubTrackers.Data;

namespace Metalitix.Scripts.Runtime.Logger.SubTrackers.Metrics
{
    public abstract class Metric
    {
        public abstract MetricData GetDataFromMetric();
        public abstract Task Initialize();
        public abstract Task Proceed();
    }
}