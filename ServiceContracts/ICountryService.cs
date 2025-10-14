using ServiceContracts.DTOs.CountryDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents the business logic for managing countries
    /// </summary>
    public interface ICountryService
    {
        /// <summary>
        /// Adds a country object to list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to be added</param>
        /// <returns>Returns the country object after adding it(including the newly generated CountryID)</returns>
        public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

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
