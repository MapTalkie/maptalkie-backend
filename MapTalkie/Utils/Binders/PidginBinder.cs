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

        public PidginBinder(bool isOptional, Parser<char, T> parser)
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
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        var result = _parser.ParseOrThrow(value);
                        bindingContext.Result = ModelBindingResult.Success(result);
                        return Task.CompletedTask;
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