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

            if (context.Metadata.ModelType.IsAssignableTo(typeof(Geometry)))
            {
                if (context.Metadata.ModelType == typeof(Point))
                {
                    return new PidginBinder<Point>(Parsers.GetInstance().Point, context.Metadata.IsNullableValueType);
                }

                if (context.Metadata.ModelType == typeof(Polygon))
                {
                    return new PidginBinder<Polygon>(Parsers.GetInstance().Polygon,
                        context.Metadata.IsNullableValueType);
                }
            }

            return null;
        }
    }
}