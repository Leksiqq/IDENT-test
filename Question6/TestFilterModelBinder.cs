using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace Question6
{
    public class TestFilterModelBinder : IModelBinder
    {
        private readonly FilterValueProvider _fallbackValueProvider = new();
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var modelName = bindingContext.ModelName;
            if (string.IsNullOrEmpty(modelName))
            {
                _fallbackValueProvider.Context = bindingContext.HttpContext;
                bindingContext.ValueProvider = _fallbackValueProvider;
            }

            var values = bindingContext.ValueProvider.GetValue(modelName);

            if (values.Length == 0)
            {
                return Task.CompletedTask;
            }

            string str = values.FirstValue.ToString();

            if (!DateTime.TryParseExact(str, "M.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(new TestFilter { YearMonth = new YearMonth { Year = dt.Year, Month = dt.Month } });

            return Task.CompletedTask;
        }
    }
    internal class FilterValueProvider : IValueProvider
    {
        internal HttpContext Context { get; set; }
        public bool ContainsPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public ValueProviderResult GetValue(string key)
        {
            if (Context.Request.Method == HttpMethods.Get)
            {
                return new ValueProviderResult(Context.Request.Query["YearMonth"] is StringValues sv && sv.Count > 0 ? sv[0] : string.Empty);
            }
            if (Context.Request.Method == HttpMethods.Post)
            {
                return new ValueProviderResult(Context.Request.Form["YearMonth"] is StringValues sv && sv.Count > 0 ? sv[0] : string.Empty);
            }
            return new ValueProviderResult(string.Empty);
        }
    }
}
