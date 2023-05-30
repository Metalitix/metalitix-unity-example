using System;
using System.Threading;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    [Serializable]
    public class DataRequest
    {
        public CancellationTokenSource Source { get; private set; }
        public string Token { get; private set; }
        public string UserID { get; private set; }
        public MetalitixProjectData MetalitixProjectData { get; private set; }
        public PageQuery Query { get; private set; }

        public DataRequest(string userID, string token, MetalitixProjectData metalitixProjectData,
            CancellationTokenSource source)
        {
            UserID = userID;
            MetalitixProjectData = metalitixProjectData;
            Token = token;
            Source = source;
        }

        public void AddQuery(PageQuery query)
        {
            if(query != null)
                Query = query;
        }
        
        public void CloseRequest()
        {
            Source.Cancel();
            Source.Dispose();
        }
    }
}