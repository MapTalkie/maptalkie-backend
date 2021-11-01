using ProjNet.CoordinateSystems.Transformations;

namespace MapTalkie.Utils
{
    public static class MapUtils
    {
        private static ICoordinateTransformation FromWGS84ToWebMercator = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(
                ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);
    }
}