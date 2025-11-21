using System;
using ServiceContracts.DTOs.CountryDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for Country entity
    /// </summary>
    public interface ICountriesGetterService
    {
        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>All countries as List of CountryResponse</returns>
        public Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Returns a country object based on the provided countryID
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns>The matching country object</returns>
        public Task<CountryResponse>? GetCountryByID(Guid? countryID);
    }
}

