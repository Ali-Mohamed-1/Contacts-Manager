using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;
using ServiceContracts.DTOs.PersonDTOs;
using Services;

namespace n12xUnit.Controllers
{
    [Route("persons")]
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountryService _countryService;
        public PersonsController(IPersonsService personsService, ICountryService countryService)
        {
            _personsService = personsService;
            _countryService = countryService;
        }

        [Route("/")]
        // [Route("index")]
        [Route("[action]")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, 
            string sortBy = nameof(PersonResponse.Name), bool isAscending = true)
        {
            // searching
            ViewBag.SearchFields = new Dictionary<string, string>
            {
                { nameof(PersonResponse.Name), "Name" },
                { nameof(PersonResponse.Email), "Email" },
                { nameof(PersonResponse.DateOfBirth), "Date Of Birth" },
                { nameof(PersonResponse.CountryName), "Country Name" },
                { nameof(PersonResponse.Gender), "Gender" },
                { nameof(PersonResponse.Address), "Address" },
                { nameof(PersonResponse.ReceiveNewsLetters), "Receive Newsletters" }
            };
            List<PersonResponse> persons = await _personsService.GetFilteredPersons(searchBy, searchString);

            // sorting
            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(persons, sortBy, isAscending);
            ViewBag.SortBy = sortBy;
            ViewBag.IsAscending = isAscending;

            ViewBag.SearchBy = searchBy;
            ViewBag.SearchString = searchString;

            return View(sortedPersons);
        }

        [Route("add")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries;

            return View();
        }

        [Route("add")]
        [HttpPost]
        public async Task<IActionResult> Add(PersonAddRequest personAddRequest)
        {
            if(!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage).ToList();

                return View(personAddRequest);
            }

            await _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToUpdateRequest();
            
            List<CountryResponse> countries = await _countryService.GetAllCountries();
            ViewBag.Countries = countries;

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByID(personUpdateRequest.PersonID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countryService.GetAllCountries();
                ViewBag.Countries = countries;

                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(msg => msg.ErrorMessage).ToList();
                return View(personResponse.ToUpdateRequest());
            }

            await _personsService.UpdatePerson(personUpdateRequest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid personID)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = await _personsService.GetPersonByID(personUpdateRequest.PersonID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            await _personsService.DeletePerson(personUpdateRequest.PersonID);

            return RedirectToAction("Index");
        }
    }
}
