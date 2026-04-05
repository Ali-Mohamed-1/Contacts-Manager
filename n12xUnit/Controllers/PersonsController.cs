using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using n12xUnit.Filters.ActionFilters;
using n12xUnit.Filters.AuthorizationFilter;
using n12xUnit.Filters.ResultFilters;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;
using ServiceContracts.DTOs.PersonDTOs;
using Services;

namespace n12xUnit.Controllers
{
    [Route("persons")]
    //[AllowAnonymous]
    public class PersonsController : Controller
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsDeleterService _personsDeleterService;


        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ILogger<PersonsController> _logger;
        public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService, IPersonsUpdaterService personsUpdaterService, IPersonsSorterService personsSorterService, IPersonsDeleterService personsDeleterService, ICountriesGetterService countriesGetterService, ILogger<PersonsController> logger)
        {
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsUpdaterService = personsUpdaterService;
            _personsSorterService = personsSorterService;
            _personsDeleterService = personsDeleterService;

            _countriesGetterService = countriesGetterService;
            _logger = logger;
        }

        [Route("/")]
        [Route("[action]")] // [Route("index")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter), 
            Arguments = new object[] { "X-Custome-Key", "Custome-Value" })]
        public async Task<IActionResult> Index(string searchBy, string? searchString, 
            string sortBy = nameof(PersonResponse.Name), bool isAscending = true)
        {
            _logger.LogInformation("Index action method of PersonsController called");
            _logger.LogDebug($"SearchBy: {searchBy}, SearchString: {searchString}, SortBy: {sortBy}, IsAscending: {isAscending}");

            // searching
            ViewBag.SearchFields = new Dictionary<string, string>
            {
                { nameof(PersonResponse.Name), "Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.CountryName), "Country Name" },
                { nameof(PersonResponse.Address), "Address" }
            };
            List<PersonResponse> persons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

            // sorting
            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(persons, sortBy, isAscending);
            
            // ViewBag parameters are moved to ActionFilter

            return View(sortedPersons);
        }

        [Route("add")]
        [HttpGet]
        [TypeFilter(typeof(ResponseHeaderActionFilter),
            Arguments = new object[] { "my-key", "my-value" })]
        public async Task<IActionResult> Add()
        {
            _logger.LogInformation("Add (GET) action method of PersonsController called");

            List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
            ViewBag.Countries = countries;

            return View();
        }

        [Route("add")]
        [HttpPost]
        [TypeFilter(typeof(PersonCreateEditPOST))]
        public async Task<IActionResult> Add(PersonAddRequest personRequest)
        {
            _logger.LogInformation("Add (POST) action method of PersonsController called");

            if(!ModelState.IsValid)
            {
                _logger.LogWarning($"ModelState is invalid in Add (POST). Errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage))}");

                List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage).ToList();

                return View(personRequest);
            }

            await _personsAdderService.AddPerson(personRequest);
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(Guid personID)
        {
            _logger.LogInformation($"Edit (GET) action method of PersonsController called for PersonID: {personID}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByID(personID);
            if (personResponse == null)
            {
                _logger.LogWarning($"Person with ID {personID} not found in Edit (GET)");
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToUpdateRequest();
            
            List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
            ViewBag.Countries = countries;

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonCreateEditPOST))]
        // [TypeFilter(typeof(TokenAuthFilter))]
        // [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            _logger.LogInformation($"Edit (POST) action method of PersonsController called for PersonID: {personRequest.PersonID}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByID(personRequest.PersonID);
            if (personResponse == null)
            {
                _logger.LogWarning($"Person with ID {personRequest.PersonID} not found in Edit (POST)");
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"ModelState is invalid in Edit (POST). Errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage))}");

                List<CountryResponse> countries = await _countriesGetterService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage).ToList();
                return View(personResponse.ToUpdateRequest());
            }

            await _personsUpdaterService.UpdatePerson(personRequest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid personID)
        {
            _logger.LogInformation($"Delete (GET) action method of PersonsController called for PersonID: {personID}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByID(personID);
            if (personResponse == null)
            {
                _logger.LogWarning($"Person with ID {personID} not found in Delete (GET)");
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            _logger.LogInformation($"Delete (POST) action method of PersonsController called for PersonID: {personUpdateRequest.PersonID}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByID(personUpdateRequest.PersonID);
            if (personResponse == null)
            {
                _logger.LogWarning($"Person with ID {personUpdateRequest.PersonID} not found in Delete (POST)");
                return RedirectToAction("Index");
            }

            await _personsDeleterService.DeletePerson(personUpdateRequest.PersonID);

            return RedirectToAction("Index");
        }
    }
}
