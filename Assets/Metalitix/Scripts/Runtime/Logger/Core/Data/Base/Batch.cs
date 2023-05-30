using System;
using System.Collections.Generic;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable]
    public class Batch
    {
        public List<Record> items { get; }

        public event Action<Record[]> OnDataPrepared;

        private const int MaxCountForValues = 20;

        public Batch()
        {
            items = new List<Record>();
        }

        public Record[] GetArray()
        {
            return items.ToArray();
        }
        
        public void Add(Record item)
        {
            
            items.Add(item);

            if (items.Count == MaxCountForValues)
            {
                OnDataPrepared?.Invoke(items.ToArray());
            }
        }
    }
}