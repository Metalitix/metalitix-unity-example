namespace Metalitix.Scripts.Runtime.Logger.SubTrackers.Data
{
    public class FpsData : MetricData
    {
        public int CurrentFrame { get; private set; }
        public int AverageFrame { get; private set; }

        public FpsData() { }

        public void SetCurrentFrame(int currentFrame)
        {
            CurrentFrame = currentFrame;
        }
        
        public void SetAverageFrame(int averageFrame)
        {
            AverageFrame = averageFrame;
        }
    }
}