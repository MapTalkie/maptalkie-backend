using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MapTalkie.Utils.EventBus
{
    public static class EventBusExtensions
    {
        public static void AddEventBus(this IServiceCollection collection)
        {
            collection.AddSingleton<IEventBus>(new LocalEventBus());
        }

        public static void Trigger(this IEventBus eventBus, object eventData)
            => eventBus.Trigger(string.Empty, eventData);


        public static IDisposable Subscribe<T>(this IEventBus eventBus, Action<T> action)
            => eventBus.Subscribe(string.Empty, action);

        public static IDisposable Subscribe<T>(this IEventBus eventBus, Func<T, Task> func)
            => eventBus.Subscribe(string.Empty, func);
    }
}