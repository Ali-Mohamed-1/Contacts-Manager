using Microsoft.AspNetCore.Mvc.Filters;
using n12xUnit.Controllers;
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

        /*
         - It can manipulate the ViewData
         - It can change the result returned from the action method
         - It can throw exceptions to either return the exception to the exception filter (if exists)
                or return the error response to the browser
        */
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");

            PersonsController personCont = (PersonsController)context.Controller;

            IDictionary<string, object?>? arguments = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];
            if(arguments != null)
            {
                if (arguments.ContainsKey("searchBy"))
                    personCont.ViewData["SearchBy"] = Convert.ToString(arguments["searchBy"]);

                if (arguments.ContainsKey("searchString"))
                    personCont.ViewData["SearchString"] = Convert.ToString(arguments["searchString"]);

                if (arguments.ContainsKey("sortBy"))
                    personCont.ViewData["sortBy"] = Convert.ToString(arguments["sortBy"]);
                
                if (arguments.ContainsKey("isAscending"))
                    personCont.ViewData["IsAscending"] = Convert.ToBoolean(arguments["isAscending"]);
            }
        }

        /*
         - It can access the action method parameters, read them & do necessary manipulations on them
         - It can validate action method parameters
         - It can short-circuit the action (prevent action method from execution) and return a different IActionResult
        */
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // indirect way to access action parameters in OnActionExecuted
            context.HttpContext.Items["arguments"] = context.ActionArguments;

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
                        nameof(PersonResponse.Address),
                        nameof(PersonResponse.CountryName)
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
