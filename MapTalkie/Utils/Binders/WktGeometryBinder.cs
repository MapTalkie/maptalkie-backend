using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetTopologySuite.IO;

namespace MapTalkie.Utils.Binders
{
    public class WktGeometryBinder : IModelBinder
    {
        private readonly bool _isOptional;
        private readonly WKTReader _reader = new();
        private readonly Type _type;

        public WktGeometryBinder(bool isOptional, Type type)
        {
            _type = type;
            _isOptional = isOptional;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
            string? error = null;

            if (valueProviderResult != ValueProviderResult.None)
            {
                var value = valueProviderResult.FirstValue;
                if (value != null)
                {
                    try
                    {
                        var geometry = _reader.Read(value);
                        if (_type.IsInstanceOfType(geometry))
                        {
                            bindingContext.Result = ModelBindingResult.Success(geometry);
                            return Task.CompletedTask;
                        }

                        error = $"Invalid geometry type, expected: {_type.Name}, got: {geometry.GetType().Name}";
                    }
                    catch (ParseException e)
                    {
                        error = e.Message;
                    }
                }
            }

            if (_isOptional)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                error ??= "unknown error";
                bindingContext.Result = ModelBindingResult.Failed();
                bindingContext.ModelState.TryAddModelError(modelName, $"Parsing error: {error}");
            }

            return Task.CompletedTask;
        }
    }
}