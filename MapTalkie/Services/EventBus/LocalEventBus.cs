using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MapTalkie.Services.EventBus.Internal;

namespace MapTalkie.Services.EventBus
{
    public class LocalEventBus : IEventBus
    {
        private const int LockTimeout = 15000;
        private readonly ConcurrentDictionary<(Type, string), List<IEventHandler>> _factories = new();

        private IDisposable Subscribe(string eventName, IEventHandler eventHandler, Type type)
        {
            WithEventHandlersLocked(type, eventName, LockTimeout, handlers => handlers.Add(eventHandler));
            return Disposable.Create(() => RemoveEventHandler(eventHandler, eventName, type));
        }

        private void RemoveEventHandler(IEventHandler eventHandler, string eventName, Type type)
        {
            WithEventHandlersLocked(type, eventName, LockTimeout, handlers => handlers.Remove(eventHandler));
        }

        #region IEventBus implementation

        public async Task Trigger(string eventName, object eventData)
        {
            var tasks = new List<Task<Exception?>>();
            var exceptions = new List<Exception>();

            WithEventHandlersLocked(
                eventData.GetType(),
                eventName,
                LockTimeout,
                eventHandlers =>
                {
                    foreach (var handler in eventHandlers)
                    {
                        var handlerType = handler.GetType();
                        if (IsAsyncEventHandler(handlerType))
                        {
                            tasks.Add(ExecuteAsyncHandler(handler, eventData, handlerType));
                        }
                        else if (IsSyncEventHandler(handlerType))
                        {
                            var exc = ExecuteSyncHandler(handler, eventData, handlerType);
                            if (exc != null)
                                exceptions.Add(exc);
                        }
                    }
                });

            exceptions.AddRange((await Task.WhenAll(tasks)).Where(exc => exc != null)!);
            // TODO handle exceptions
        }

        private static Exception? ExecuteSyncHandler(IEventHandler handler, object eventData, Type handlerType)
        {
            try
            {
                var method = handlerType.GetMethod(nameof(ISyncEventHandler<object>.HandleEvent));
                method!.Invoke(handler, new[] { eventData });
                return null;
            }
            catch (TargetInvocationException e)
            {
                return e.InnerException ?? e;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private static async Task<Exception?> ExecuteAsyncHandler(IEventHandler handler, object eventData,
            Type handlerType)
        {
            try
            {
                var method = handlerType.GetMethod(nameof(IEventHandler<object>.HandleEvent));
                await (Task)method!.Invoke(handler, new[] { eventData })!;
                return null;
            }
            catch (TargetInvocationException e)
            {
                return e.InnerException ?? e;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private static bool IsAsyncEventHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }

        private static bool IsSyncEventHandler(Type handlerType)
        {
            return handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(ISyncEventHandler<>));
        }

        private void WithEventHandlersLocked(Type eventType, string eventName, int timeout,
            Action<List<IEventHandler>> factoriesFn)
        {
            List<IEventHandler> factories;
            bool hasHandlers = true;
            var key = (eventType, eventName);
            lock (_factories)
            {
                if (!_factories.ContainsKey(key))
                {
                    factories = new();
                    hasHandlers = false;
                }

                factories = _factories[key];
            }

            if (Monitor.TryEnter(factories, timeout))
            {
                try
                {
                    factoriesFn(factories);
                }
                finally
                {
                    Monitor.Exit(factories);
                }
            }
            else
            {
                throw new TimeoutException();
            }

            if (!hasHandlers && factories.Count != 0)
            {
                lock (_factories)
                {
                    _factories[key] = factories;
                }
            }
        }

        public IDisposable Subscribe<T>(string eventName, Action<T> action, bool onThreadPool = false)
        {
            return Subscribe(
                eventName,
                onThreadPool ? new SyncToAsyncOnThreadPoolEventHandler<T>(action) : new SyncEventHandler<T>(action),
                typeof(T));
        }

        public IDisposable Subscribe<T>(string eventName, Func<T, Task> func)
        {
            return Subscribe(
                eventName,
                new AsyncEventHandler<T>(func), typeof(T));
        }

        #endregion
    }
}