using System.Threading;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;

namespace Metalitix.Scripts.Editor.Dashboard.Requests
{
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
}