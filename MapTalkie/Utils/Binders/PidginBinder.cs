using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Pidgin;

namespace MapTalkie.Utils.Binders
{
    public class PidginBinder<T> : IModelBinder
    {
        private readonly bool _isOptional;
        private readonly Parser<char, T> _parser;

        public PidginBinder(Parser<char, T> parser, bool isOptional)
        {
            _parser = parser;
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
                    var result = _parser.Parse(value);

                    if (result.Success)
                    {
                        bindingContext.Result = ModelBindingResult.Success(result.Value);
                        return Task.CompletedTask;
                    }

                    error = result.Error!.Message;
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