using System;
using Metalitix.Core.Data.Runtime;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Logger.Core.Base.Senders
{
    [Serializable]
    public class HttpRecordWrapper
    {
        public string appkey { get; private set; }
        public string apiver = "v2";
        public Record[] items { get; private set; }

        [JsonConstructor]
        public HttpRecordWrapper(string appkey, Record[] items)
        {
            this.appkey = appkey;
            this.items = items;
        }
    }
}