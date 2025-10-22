using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Entities;
using ServiceContracts;
using Entities.Data;
using ServiceContracts.DTOs;
using ServiceContracts.DTOs.CountryDTOs;
using Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using EntityFrameworkCoreMock;


namespace CRUDtest
{
    public class CountriesServiceTests
    {
        private readonly ICountryService _countryService;
        public CountriesServiceTests()
        {
            var countriesInitialData = new List<Country>() { };

            DbContextMock<AppDbContext> dbContextMock = new DbContextMock<AppDbContext>(
                new DbContextOptionsBuilder<AppDbContext>()
                    .Options);

            var dbContext = dbContextMock.Object; // Mock AppDbContext
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            _countryService =  new CountriesService(null);
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
        public async Task AddCountry_DuplicateCountryName()
        {
            // arrange
            CountryAddRequest? request1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest? request2 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };

            // assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // act
                await Task.Run(() => _countryService.AddCountry(request1));
                await Task.Run(() => _countryService.AddCountry(request2));
            });
        }

        // when supplying valid country object => insert the country to the list
        [Fact]
        public async Task AddCountry_ProperCountryObject()
        {
            // arrange
            CountryAddRequest? request = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };

            // act
            CountryResponse response = await Task.Run(() => _countryService.AddCountry(request));
            List<CountryResponse> countries = await Task.Run(() => _countryService.GetAllCountries());

            // assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(countries, country =>
                country.CountryID == response.CountryID
                    && country.CountryName == response.CountryName);
        }

        #endregion

        #region GetAllCountries tests

        // coutries list should be empty initially
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // arrange
            // act
            List<CountryResponse> countries = await Task.Run(() => _countryService.GetAllCountries());
            // assert
            Assert.Empty(countries);
        }

        [Fact]
        public async Task GetAllCountries_NonEmptyList()
        {
            // arrange
            CountryAddRequest? request1 = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryAddRequest? request2 = new CountryAddRequest()
            {
                CountryName = "USA"
            };
            
            List<CountryResponse> expectedCountries = new List<CountryResponse>()
            {
                await Task.Run(() => _countryService.AddCountry(request1)),
                await Task.Run(() => _countryService.AddCountry(request2))
            };

            // act
            List<CountryResponse> actualCountries = await Task.Run(() => _countryService.GetAllCountries());
            
            // assert
            Assert.Equal(2, actualCountries.Count);
            foreach (CountryResponse expectedCountry in expectedCountries)
            {
                // Assert.Contains(expectedCountry, actualCountries);

                // more robust way of checking
                Assert.Contains(actualCountries, country => 
                    country.CountryID == expectedCountry.CountryID 
                        && country.CountryName == expectedCountry.CountryName);
            }
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
            CountryAddRequest request = new CountryAddRequest()
            {
                CountryName = "Egypt"
            };
            CountryResponse country = await Task.Run(() => _countryService.AddCountry(request));

            // act
            CountryResponse? expectedCoutry = await Task.Run(() => _countryService.GetCountryByID(country.CountryID));

            // assert
            Assert.Equal(expectedCoutry.CountryID, country.CountryID);
        }

        #endregion
    }
}
