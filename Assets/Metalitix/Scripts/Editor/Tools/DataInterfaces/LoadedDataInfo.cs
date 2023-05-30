using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public class LoadedDataInfo
    {
        public List<Record> Records { get; }
        public DataRequest Request { get; }

        public LoadedDataInfo(List<Record> records, DataRequest request)
        {
            Records = records;
            Request = request;
        }
    }
}