using Microsoft.AspNetCore.Mvc.Filters;

namespace n12xUnit.Filters.ActionFilters
{
    public class ResponseHeaderActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<ResponseHeaderActionFilter> _logger;
        private readonly string _key;
        private readonly string _value;
        public ResponseHeaderActionFilter(ILogger<ResponseHeaderActionFilter> logger, string key, string value)
        {
            _logger = logger;
            _key = key;
            _value = value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // before logic
            _logger.LogInformation("{filterName}.OnActionExecutionAsync method", nameof(ResponseHeaderActionFilter));

            await next();

            // after logic
            _logger.LogInformation("{filterName}.OnActionExecutionAsync method", nameof(ResponseHeaderActionFilter));
            context.HttpContext.Response.Headers[_key] = _value;
        }
    }
}
