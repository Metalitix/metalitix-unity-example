using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Settings;
using Metalitix.Core.Tools.RequestTools;

namespace Metalitix.Scripts.Logger.Core.Base.Senders
{
    public abstract class DataSender<T>
    {
        protected readonly GlobalSettings GlobalSettings;
        protected readonly WebRequestHelper WebRequestHelper;

        protected DataSender(GlobalSettings globalSettings, WebRequestHelper webRequestHelper)
        {
            GlobalSettings = globalSettings;
            WebRequestHelper = webRequestHelper;
        }

        public abstract Task<string> InitializeSession();
        public abstract Task SendData(T[] data, CancellationToken token = new CancellationToken());
    }
}