using System;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class BackendData
    {
        public Pagination pagination { get; private set; }

        public BackendData(Pagination pagination)
        {
            this.pagination = pagination;
        }
    }
}