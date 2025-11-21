using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using n12xUnit.Controllers;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;
using ServiceContracts.DTOs.PersonDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDtest
{
    public class PersonControllerTests
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ILogger<PersonsController> _logger;

        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
        private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly IFixture _fixture;

        public PersonControllerTests()
        {
            _fixture = new Fixture();

            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsGetterService = _personsGetterServiceMock.Object;

            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _personsAdderService = _personsAdderServiceMock.Object;

            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsUpdaterService = _personsUpdaterServiceMock.Object;

            _personsSorterServiceMock = new Mock<IPersonsSorterService>();
            _personsSorterService = _personsSorterServiceMock.Object;

            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsDeleterService = _personsDeleterServiceMock.Object;

            _countriesGetterServiceMock = new Mock<ICountriesGetterService>();
            _countriesGetterService = _countriesGetterServiceMock.Object;

            _loggerMock = new Mock<ILogger<PersonsController>>();
            _logger = _loggerMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithListOfPersons()
        {
            // Arrange
            List<PersonResponse> personResponse_list = _fixture.Create<List<PersonResponse>>();
            
            PersonsController controller = new PersonsController(_personsGetterService, _personsAdderService, _personsUpdaterService, _personsSorterService, _personsDeleterService, _countriesGetterService, _logger);

            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personResponse_list);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(personResponse_list);

            // Act
            IActionResult result = await controller.Index("", "", "", true);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<List<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(personResponse_list);
        }

        #endregion

        #region Add

        [Fact]
        public async Task Add_IfModelErrors_ToReturnCreateView()
        {
            // Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController controller = new PersonsController(_personsGetterService, _personsAdderService, _personsUpdaterService, _personsSorterService, _personsDeleterService, _countriesGetterService, _logger);

            _countriesGetterServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            // Act
            controller.ModelState.AddModelError("Name", "Name is required");

            IActionResult result = await controller.Add(personAddRequest);

            // Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);
            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            viewResult.ViewData.Model.Should().Be(personAddRequest);
        }

        [Fact]
        public async Task Add_IfNoModelErrors_ToRedirectToIndex()
        {
            // Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();
            List<CountryResponse> countries = _fixture.Create<List<CountryResponse>>();

            PersonsController controller = new PersonsController(_personsGetterService, _personsAdderService, _personsUpdaterService, _personsSorterService, _personsDeleterService, _countriesGetterService, _logger);

            _countriesGetterServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            // Act
            IActionResult result = await controller.Add(personAddRequest);

            // Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion
    }
}
