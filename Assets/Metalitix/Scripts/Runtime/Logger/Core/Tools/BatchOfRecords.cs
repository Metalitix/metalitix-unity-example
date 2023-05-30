using System.Collections.Generic;
using Metalitix.Scripts.Runtime.Logger.Core.Data.Base;

namespace Metalitix.Scripts.Runtime.Logger.Core.Tools
{
    public class BatchOfRecords
    {
        private int _count;
        private readonly List<Record> _records;
        
        public bool IsBatchFulled { get; private set; }

        public BatchOfRecords(int count)
        {
            _count = count;
            _records = new List<Record>();
        }

        public void AddRecord(Record record)
        {
            if (_records.Count == _count)
            {
                IsBatchFulled = true;
                return;
            }
            
            _records.Add(record);
            CheckFulled();
        }

        public void Release(Record endRecord)
        {
            _records.Add(endRecord);
        }

        public Record[] FinalizeBatch()
        {
            return _records.ToArray();
        }

        private void CheckFulled()
        {
            if (_records.Count == _count)
            {
                IsBatchFulled = true;
            }
        }
    }
}