using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Entities;
using ServiceContracts;
using ServiceContracts.DTOs;
using ServiceContracts.DTOs.CountryDTOs;
using Services;
using Moq;
using RepositoryContracts;
using AutoFixture;
using Azure.Core;
using FluentAssertions;


namespace CRUDtest
{
    public class CountriesServiceTests
    {
        private readonly ICountryService _countryService;
        private readonly Mock<ICountriesRepository> _countriesRepoMock;
        private readonly ICountriesRepository _countriesRepo;
        private readonly IFixture _fixture;

        public CountriesServiceTests()
        {
            _countriesRepoMock = new Mock<ICountriesRepository>();
            _countriesRepo = _countriesRepoMock.Object;

            _countryService = new CountriesService(_countriesRepo);

            _fixture = new Fixture();
        }
         
        #region AddCountry tests

        // when null => throws ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountryObject()
        {
            // arrange
            CountryAddRequest? request = null;

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                // act
                await Task.Run(() => _countryService.AddCountry(request));
            });
        }

        // when CountryName is null => throws ArgumentE[Fact]
        [Fact]
        public async Task AddCountry_NullCountryName()
        {
            // arrange
            CountryAddRequest? request = new CountryAddRequest()
            {
                CountryName = null
            };

            // assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // act
                await Task.Run(() => _countryService.AddCountry(request));
            });
        }

        // when CountryName is duplicated => throws ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ShouldThrowArgumentException()
        {
            // arrange
            var existingCountry = _fixture.Build<Country>()
                .With(c => c.CountryName, "Egypt")
                .Create();

            var countryToAdd = _fixture.Build<CountryAddRequest>()
                .With(c => c.CountryName, "Egypt")
                .Create();

            _countriesRepoMock
                .Setup(repo => repo.GetCountryByName("Egypt"))
                .ReturnsAsync(existingCountry);

            // assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // act
                await Task.Run(() => _countryService.AddCountry(countryToAdd));
            });
        }


        // when supplying valid country object => insert the country to the list
        [Fact]
        public async Task AddCountry_ProperCountryObject_ToBeSucessful()
        {
            // arrange
            var request = new CountryAddRequest
            {
                CountryName = "Egypt"
            };

            _countriesRepoMock
                .Setup(repo => repo.GetCountryByName("Egypt"))
                .ReturnsAsync((Country?)null);

            _countriesRepoMock
                .Setup(repo => repo.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(new Country());

            // act
            var response = await _countryService.AddCountry(request);

            // assert
            response.Should().NotBeNull();
            response.CountryID.Should().NotBe(Guid.Empty);
            response.CountryName.Should().Be("Egypt");
        }

        #endregion

        #region GetAllCountries tests

        // coutries list should be empty initially
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // arrange
            _countriesRepoMock.Setup(repo => repo.GetAllCountries())
                .ReturnsAsync(new List<Country>());

            // act
            List<CountryResponse> countries = await Task.Run(() => _countryService.GetAllCountries());
            // assert
            Assert.Empty(countries);
        }

        [Fact]
        public async Task GetAllCountries_NonEmptyList()
        {
            // arrange
            List<Country> countries = new List<Country>()
            {
                new Country { CountryName = "Egypt", CountryID = Guid.NewGuid() },
                new Country { CountryName = "USA", CountryID = Guid.NewGuid() }
            };

            _countriesRepoMock.Setup(repo => repo.GetAllCountries())
                .ReturnsAsync(countries);

            // act
            List<CountryResponse> actualCountries = await Task.Run(() => _countryService.GetAllCountries());
            
            // assert
            Assert.Equal(2, actualCountries.Count);
            Assert.Contains(actualCountries, country => country.CountryName == "Egypt");
            Assert.Contains(actualCountries, country => country.CountryName == "USA");
        }

        #endregion

        #region GetCountryByID tests

        // when countryID is null => return null
        [Fact]
        public async Task GetCountryByID_NullCountryID()
        {
            // arrange
            Guid? countryID = null;
            // act
            CountryResponse? country = await Task.Run(() => _countryService.GetCountryByID(countryID));
            // assert
            Assert.Null(country);
        }

        // when countryID is valid => return matching country object
        [Fact]
        public async Task GetCountryByID_ValidCountryID()
        {
            // arrange
            Guid countryID = Guid.NewGuid();
            Country country = new Country { CountryName = "Egypt", CountryID = countryID };

            _countriesRepoMock.Setup(repo => repo.GetCountryByID(countryID))
                .ReturnsAsync(country);

            // act
            CountryResponse? expectedCountry = await Task.Run(() => _countryService.GetCountryByID(countryID));

            // assert
            Assert.NotNull(expectedCountry);
            Assert.Equal(countryID, expectedCountry.CountryID);
            Assert.Equal("Egypt", expectedCountry.CountryName);
        }

        #endregion
    }
}
