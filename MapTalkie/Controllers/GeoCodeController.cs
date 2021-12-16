using System.Threading.Tasks;
using MapTalkie.Services.GeoService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace MapTalkie.Controllers
{
    [ApiController, Route("[controller]")]
    public class GeoCodeController : Controller
    {
        [HttpGet("location")]
        public async Task<ActionResult<dynamic>> GetLocation(
            [FromServices] IGeoService geoService,
            [FromServices] IMemoryCache cache)
        {
            Point point;
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            if (ipAddress != null)
            {
                var key = $"ipLoc{HttpContext.Connection.RemoteIpAddress}";
                if (!cache.TryGetValue(key, out point))
                {
                    point = await geoService.FindIpLocation(ipAddress);
                    cache.Set(key, point);
                }
            }
            else
            {
                point = geoService.GetDefaultLocation();
            }

            return new
            {
                Location = point,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
        }
    }
}