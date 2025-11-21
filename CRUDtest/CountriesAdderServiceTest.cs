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
    public class CountriesAdderServiceTest
    {
        private readonly ICountriesAdderService _countriesAdderService;
        private readonly Mock<ICountriesRepository> _countriesRepoMock;
        private readonly ICountriesRepository _countriesRepo;
        private readonly IFixture _fixture;
        private readonly Mock<ILogger<CountriesAdderService>> _adderLoggerMock;

        public CountriesAdderServiceTest()
        {
            _countriesRepoMock = new Mock<ICountriesRepository>();
            _countriesRepo = _countriesRepoMock.Object;

            _adderLoggerMock = new Mock<ILogger<CountriesAdderService>>();

            _countriesAdderService = new CountriesAdderService(_countriesRepo, _adderLoggerMock.Object);

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
                await Task.Run(() => _countriesAdderService.AddCountry(request));
            });
        }

        // when CountryName is null => throws ArgumentException
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
                await Task.Run(() => _countriesAdderService.AddCountry(request));
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
                await Task.Run(() => _countriesAdderService.AddCountry(countryToAdd));
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
            var response = await _countriesAdderService.AddCountry(request);

            // assert
            response.Should().NotBeNull();
            response.CountryID.Should().NotBe(Guid.Empty);
            response.CountryName.Should().Be("Egypt");
        }

        #endregion
    }
}

