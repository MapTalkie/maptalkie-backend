using System.Collections.Generic;
using MapTalkie.Models;

namespace MapTalkie.Services.PostService
{
    public class MapLayerState
    {
        public List<MapCluster> Clusters { get; set; } = new();
        public List<MapPost> Popular { get; set; } = new();
    }
}