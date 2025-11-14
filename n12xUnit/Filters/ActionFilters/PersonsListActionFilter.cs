using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTOs.PersonDTOs;

namespace n12xUnit.Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

            if (context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["SearchBy"]);
               
                // validating searchBy parameter value
                if (!string.IsNullOrEmpty(searchBy))
                {
                    var searchByOptions = new List<string>()
                    {
                        nameof(PersonResponse.Name),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryID),
                        nameof(PersonResponse.Address)
                    };

                    if(searchByOptions.Any(temp => temp == searchBy) == false)
                    {
                        _logger.LogInformation("searchBy actual value is {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.Name);
                        _logger.LogInformation("searchBy updated value is {searchBy}", searchBy);
                    }
                }
            }
        }
    }
}
