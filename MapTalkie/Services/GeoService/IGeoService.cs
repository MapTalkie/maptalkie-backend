using System.Net;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.GeoService
{
    public interface IGeoService
    {
        Task<Point> FindIpLocation(IPAddress ipAddress);
        Point GetDefaultLocation();
    }
}