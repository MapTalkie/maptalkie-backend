namespace MapTalkie.Services.PostService
{
    public class LatLng
    {
        private LatLng(float lat, float lng)
        {
            Latitude = lat;
            Longitude = lng;
        }

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public LatLng From(float lat, float lng)
        {
            return new LatLng(lat, lng);
        }
    }
}