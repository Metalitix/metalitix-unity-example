using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Extensions;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataPatchers
{
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
}