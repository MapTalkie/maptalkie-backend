using System;
using System.Threading.Tasks;

namespace MapTalkie.Utils.EventBus.Internal
{
    public class SyncToAsyncOnThreadPoolEventHandler<T> : IEventHandler<T>
    {
        private readonly Action<T> _syncAction;

        public SyncToAsyncOnThreadPoolEventHandler(Action<T> syncHandler)
        {
            _syncAction = syncHandler;
        }

        public Task HandleEvent(T value)
        {
            return Task.Run(() => _syncAction(value));
        }
    }
}