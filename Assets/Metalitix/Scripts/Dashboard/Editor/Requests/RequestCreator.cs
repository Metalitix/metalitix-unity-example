using System.Threading;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;

namespace Metalitix.Scripts.Dashboard.Editor.Requests
{
#if UNITY_EDITOR
    public abstract class RequestCreator
    {
        protected readonly string AuthToken;
        protected readonly string UserID;
        protected readonly CancellationTokenSource Source;
        protected readonly MetalitixBridge MetalitixBridge;
        protected PageQuery PageQuery;

        public RequestCreator(DataRequest request, MetalitixBridge metalitixBridge)
        {
            MetalitixBridge = metalitixBridge;
            Source = request.Source;
            AuthToken = request.Token;
            UserID = request.UserID;
            PageQuery = request.Query;
        }
    }
#endif
}