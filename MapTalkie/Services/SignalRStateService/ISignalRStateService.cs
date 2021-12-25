using System.Threading.Tasks;

namespace MapTalkie.Services.SignalRStateService;

public interface ISignalRStateService
{
    Task AddConnection(string userId, string connectionId);
    Task RemoveConnection(string connectionId);
}