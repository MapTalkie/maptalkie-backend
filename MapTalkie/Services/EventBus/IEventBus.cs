using System;
using System.Threading.Tasks;

namespace MapTalkie.Services.EventBus
{
    public interface IEventBus
    {
        /// <summary>
        /// Вызывает событие с указанным именем и данными.
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <param name="eventData">Данные события, не может быть null</param>
        Task Trigger(string eventName, object eventData);

        /// <summary>
        /// Подписывается на событие синхронно.
        /// </summary>
        /// <param name="action">Действие, которое нужно совершить</param>
        /// <param name="eventName">Имя события</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDisposable Subscribe<T>(string eventName, Action<T> action, bool onThreadPool = false);

        /// <summary>
        /// Подписывается на событие асинхронно.
        /// </summary>
        /// <param name="func">Функция возвращающая задачу</param>
        /// <typeparam name="T">Тип данных события</typeparam>
        /// <returns>Объект подписки</returns>
        IDisposable Subscribe<T>(string eventName, Func<T, Task> func);
    }
}