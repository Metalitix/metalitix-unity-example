using System;
using Newtonsoft.Json;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class Pagination
    {
        public int totalItemsCount { get; set; }
        public int totalPagesCount { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }

        [JsonConstructor]
        public Pagination(int totalItemsCount, int totalPagesCount, int currentPage, int pageSize)
        {
            this.totalItemsCount = totalItemsCount;
            this.totalPagesCount = totalPagesCount;
            this.currentPage = currentPage;
            this.pageSize = pageSize;
        }
    }
}