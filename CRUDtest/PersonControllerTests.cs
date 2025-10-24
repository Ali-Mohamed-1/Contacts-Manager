using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IPersonsService _personsService;
        private readonly ICountryService _countryService; 

        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ICountryService> _countryServiceMock;

        private readonly IFixture _fixture;

        public PersonControllerTests()
        {
            _fixture = new Fixture();

            _personsServiceMock = new Mock<IPersonsService>();
            _personsService = _personsServiceMock.Object;

            _countryServiceMock = new Mock<ICountryService>();
            _countryService = _countryServiceMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithListOfPersons()
        {
            // Arrange
            List<PersonResponse> personResponse_list = _fixture.Create<List<PersonResponse>>();
            
            PersonsController controller = new PersonsController(_personsService, _countryService);

            _personsServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personResponse_list);

            _personsServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<bool>()))
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

            PersonsController controller = new PersonsController(_personsService, _countryService);

            _countryServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
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

            PersonsController controller = new PersonsController(_personsService, _countryService);

            _countryServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
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
