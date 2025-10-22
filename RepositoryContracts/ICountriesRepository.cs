using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents data access logic for Countries
    /// </summary>
    public interface ICountriesRepository
    {
        Task<Country> AddCountry(Country country);

        Task<List<Country>> GetAllCountries();

        Task<Country?> GetCountryByID(Guid countryID); 

        Task<Country?> GetCountryByName(string CountryName); 
    }
}
