namespace MapTalkie.Services.PostService
{
    public class LatLngBounds
    {
        private LatLngBounds(LatLng southWest, LatLng northEast)
        {
            SouthWest = southWest;
            NorthEast = northEast;
        }

        public LatLng SouthWest { get; }
        public LatLng NorthEast { get; }

        public LatLngBounds From(LatLng southWest, LatLng northEast)
        {
            return new LatLngBounds(southWest, northEast);
        }
    }
}