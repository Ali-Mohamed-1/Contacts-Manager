using System;
using ServiceContracts.DTOs.CountryDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for Country entity
    /// </summary>
    public interface ICountriesAdderService
    {
        /// <summary>
        /// Adds a country object to list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to be added</param>
        /// <returns>Returns the country object after adding it(including the newly generated CountryID)</returns>
        public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);
    }
}

