using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Utils.MapUtils;

namespace MapTalkie.Services.PostService.HotPosts
{
    public interface IHotPosts
    {
        Task ReportPostPopularityChange(MapPost post);

        Task UpdateZone(MapZoneDescriptor zoneDescriptor);
    }
}