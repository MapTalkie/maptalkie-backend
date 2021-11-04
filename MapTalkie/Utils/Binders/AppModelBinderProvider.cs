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
                return new WktGeometryBinder(context.Metadata.IsNullableValueType, context.Metadata.ModelType);
            }

            return null;
        }
    }
}