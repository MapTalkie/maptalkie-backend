using System.Threading.Tasks;

namespace MapTalkie.Services.EventBus
{
    public interface IEventHandler
    {
    }

    public interface IEventHandler<in T> : IEventHandler
    {
        Task HandleEvent(T value);
    }

    public interface ISyncEventHandler<in T> : IEventHandler
    {
        void HandleEvent(T value);
    }
}