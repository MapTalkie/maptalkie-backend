using System;
using System.Threading.Tasks;

namespace MapTalkie.Utils.EventBus.Internal
{
    public class AsyncEventHandler<T> : IEventHandler<T>
    {
        private readonly Func<T, Task> _func;

        public AsyncEventHandler(Func<T, Task> func)
        {
            _func = func;
        }

        public Task HandleEvent(T value)
        {
            return _func(value);
        }
    }
}