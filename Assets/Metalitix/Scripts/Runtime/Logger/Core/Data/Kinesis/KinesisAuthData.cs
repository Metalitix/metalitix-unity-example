using System;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Kinesis
{
    [Serializable]
    public class KinesisAuthData
    {
        public string appkey { get; }

        public KinesisAuthData(string appKey)
        {
            this.appkey = appKey;
        }
    }
}