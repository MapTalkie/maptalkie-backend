using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.GeoService;

public class TwoGISGeoService : IGeoService
{
    private static readonly Point _defaultLocation = new(37.618423, 55.751244) { SRID = 4326 };
    private readonly HttpClient _client;
    private readonly TwoGisOptions _options;

    public TwoGISGeoService(IOptions<TwoGisOptions> twoGisOptions)
    {
        _options = twoGisOptions.Value;
        _client = new HttpClient();
    }

    public async Task<Point> FindIpLocation(IPAddress ipAddress)
    {
        try
        {
            var response = await _client.GetAsync(
                $"https://catalog.api.2gis.com/3.0/items/geocode/byip?key={_options.Token}&ip={ipAddress}&type=coordinates");
            var data = await response.Content.ReadFromJsonAsync<TwoGisResponse>();
            var item = data!.Result.Items[0];
            return new Point(item.Lon, item.Lat) { SRID = 4326 };
        }
        catch (Exception e)
        {
            return GetDefaultLocation();
        }
    }

    public Point GetDefaultLocation()
    {
        return _defaultLocation;
    }

    private class TwoGisResponse
    {
        public TwoGisResponseResult Result { get; } = null!;
    }

    private class TwoGisResponseResult
    {
        public List<TwoGisItem> Items { get; } = null!;
    }

    private class TwoGisItem
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}