using System.Threading.Tasks;
using Metalitix.Core.Tools;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;

namespace Metalitix.Scripts.Dashboard.Editor.Requests.DataPatchers
{
#if UNITY_EDITOR
    public abstract class DataPatcher<T, TV> : RequestCreator
    {
        protected PageQuery PageQuery;
        protected T UpdatedData;
        protected TV LoadedData;

        public DataPatcher(T updatedData, DataRequest request, MetalitixBridge metalitixBridge) : base(request, metalitixBridge)
        {
        }

        public abstract Task<TV> UpdateAndReturn();
        public abstract Task UpdateData();

        protected void LogSuccessful()
        {
            MetalitixDebug.Log(this, "Data patched successfully!");
        }
    }
#endif
}