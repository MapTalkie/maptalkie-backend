using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Utils.MapUtils;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.EventBus
{
    public static class EventBusExtensions
    {
        public static void AddEventBus(this IServiceCollection collection)
        {
            collection.AddSingleton<IEventBus, LocalEventBus>();
        }

        #region Multi-trigger

        public static Task Trigger(this IEventBus eventBus, IEnumerable<string> eventNames, object data)
            => Task.WhenAll(eventNames.Select(eventName => eventBus.Trigger(eventName, data)));

        #endregion

        #region Event bus shortcuts

        public static Task Trigger(this IEventBus eventBus, object eventData)
            => eventBus.Trigger(string.Empty, eventData);

        public static Task Trigger(this IEventBus eventBus, long eventId, object eventData)
            => eventBus.Trigger(eventId.ToString(), eventData);

        public static IDisposable Subscribe<T>(this IEventBus eventBus, Action<T> action)
            => eventBus.Subscribe(string.Empty, action);

        public static IDisposable Subscribe<T>(this IEventBus eventBus, Func<T, Task> func)
            => eventBus.Subscribe(string.Empty, func);

        public static IDisposable Subscribe<T>(this IEventBus eventBus, long eventId, Func<T, Task> func)
            => eventBus.Subscribe(eventId.ToString(), func);

        #endregion

        #region Spatial event bus

        public static IDisposable Subscribe<T>(
            this IEventBus eventBus,
            MapZoneDescriptor descriptor,
            string eventName,
            Action<T> action)
            => eventBus.Subscribe($"AREA.{descriptor.ToIdentifier()}.{eventName}", action);

        public static IDisposable Subscribe<T>(
            this IEventBus eventBus,
            MapZoneDescriptor descriptor,
            string eventName,
            Func<T, Task> func)
            => eventBus.Subscribe($"AREA.{descriptor.ToIdentifier()}.{eventName}", func);

        public static IDisposable Subscribe<T>(
            this IEventBus eventBus,
            Polygon polygon,
            string eventName,
            Func<T, Point> pointFunction,
            Action<T> action)
        {
            var descriptor = MapUtils.GetZone(polygon);

            void AlteredAction(T v)
            {
                if (polygon.Contains(pointFunction(v)))
                {
                    action(v);
                }
            }

            return eventBus.Subscribe<T>(descriptor, eventName, AlteredAction);
        }

        public static IDisposable Subscribe<T>(
            this IEventBus eventBus,
            Polygon polygon,
            string eventName,
            Func<T, Point> pointFunction,
            Func<T, Task> func)
        {
            var descriptor = MapUtils.GetZone(polygon);

            Task AlteredAction(T v)
            {
                if (polygon.Contains(pointFunction(v)))
                {
                    return func(v);
                }

                return Task.CompletedTask;
            }

            return eventBus.Subscribe<T>(descriptor, eventName, AlteredAction);
        }

        public static Task Trigger(this IEventBus eventBus, Point point, string eventName, object data)
        {
            var tasks = MapUtils.GetZones(point)
                .Select(descriptor => eventBus.Trigger($"AREA.{descriptor.ToIdentifier()}.{eventName}", data));
            return Task.WhenAll(tasks);
        }

        #endregion
    }
}