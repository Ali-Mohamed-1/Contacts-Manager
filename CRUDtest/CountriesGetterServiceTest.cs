using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Entities;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;
using Services;
using Moq;
using RepositoryContracts;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CRUDtest
{
    public class CountriesGetterServiceTest
    {
        private readonly ICountriesGetterService _countriesGetterService;
        private readonly Mock<ICountriesRepository> _countriesRepoMock;
        private readonly ICountriesRepository _countriesRepo;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<CountriesGetterService>> _getterLoggerMock;

        public CountriesGetterServiceTest()
        {
            _countriesRepoMock = new Mock<ICountriesRepository>();
            _countriesRepo = _countriesRepoMock.Object;

            _getterLoggerMock = new Mock<ILogger<CountriesGetterService>>();

            _countriesGetterService = new CountriesGetterService(_countriesRepo, _getterLoggerMock.Object);

            _fixture = new Fixture();
        }

        #region GetAllCountries tests

        // coutries list should be empty initially
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // arrange
            _countriesRepoMock.Setup(repo => repo.GetAllCountries())
                .ReturnsAsync(new List<Country>());

            // act
            List<CountryResponse> countries = await Task.Run(() => _countriesGetterService.GetAllCountries());
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
            List<CountryResponse> actualCountries = await Task.Run(() => _countriesGetterService.GetAllCountries());
            
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
            CountryResponse? country = await Task.Run(() => _countriesGetterService.GetCountryByID(countryID));
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
            CountryResponse? expectedCountry = await Task.Run(() => _countriesGetterService.GetCountryByID(countryID));

            // assert
            Assert.NotNull(expectedCountry);
            Assert.Equal(countryID, expectedCountry.CountryID);
            Assert.Equal("Egypt", expectedCountry.CountryName);
        }

        #endregion
    }
}

