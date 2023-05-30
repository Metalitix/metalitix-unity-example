using System.Threading;
using System.Threading.Tasks;

namespace Metalitix.Scripts.Runtime.Logger.Core.Base
{
    public abstract class RecordsSender
    {
        protected readonly string appKeyCode;

        protected RecordsSender(string appKeyCode)
        {
            this.appKeyCode = appKeyCode;
        }

        public abstract Task<string> InitializeSession();
        public abstract Task SendData(object data, CancellationToken token = new());
    }
}