using Microsoft.AspNetCore.Mvc.Filters;
using n12xUnit.Controllers;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;
using ServiceContracts.DTOs.PersonDTOs;

namespace n12xUnit.Filters.ActionFilters
{
    public class PersonCreateEditPOST : IAsyncActionFilter
    {
        private readonly ICountriesGetterService _countriesGetterService;
        public PersonCreateEditPOST(ICountriesGetterService countriesGetterService)
        {
            _countriesGetterService = countriesGetterService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is PersonsController personsController)
            {
                if (!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
                    personsController.ViewBag.Countries = countries;

                    personsController.ViewBag.Errors = personsController.ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage).ToList();

                    var personRequest = context.ActionArguments["personRequest"];
                    context.Result = personsController.View(personRequest); // skips the next filters
                }
                else
                {
                    await next();
                }
            }
            else
            {
                await next();
            }
        }
    }
}
