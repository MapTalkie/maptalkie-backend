using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetTopologySuite.Geometries;

namespace MapTalkie.Utils.Binders
{
    public class AppModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var type = context.Metadata.ModelType;
            if (type == typeof(Polygon))
            {
                return new PidginBinder<Polygon>(context.Metadata.IsReferenceOrNullableType, Parsers.Polygon);
            }

            if (type == typeof(Point))
            {
                return new PidginBinder<Point>(context.Metadata.IsReferenceOrNullableType, Parsers.LatLonPoint);
            }

            return null;
        }
    }
}