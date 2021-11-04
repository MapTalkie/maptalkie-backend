using System;

namespace MapTalkie.Utils.EventBus.Internal
{
    public class SyncEventHandler<T> : ISyncEventHandler<T>
    {
        private readonly Action<T> _action;

        public SyncEventHandler(Action<T> action)
        {
            _action = action;
        }

        public void HandleEvent(T value)
        {
            _action(value);
        }
    }
}