using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;

namespace Repositories
{
    public class CountriesRepo : ICountriesRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CountriesRepo> _logger;

        public CountriesRepo(AppDbContext db, ILogger<CountriesRepo> logger)
        {
            _db = db;
            _logger = logger;
        }


        public async Task<Country> AddCountry(Country country)
        {
            _logger.LogInformation("AddCountry method called in CountriesRepo");

            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            return country;
        }

        public async Task<List<Country>> GetAllCountries()
        {
            _logger.LogInformation("GetAllCountries method called in CountriesRepo");

            return await _db.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryByID(Guid countryID)
        {
            _logger.LogInformation("GetCountryByID method called in CountriesRepo");

            return await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryID);
        }

        public async Task<Country?> GetCountryByName(string CountryName)
        {
            _logger.LogInformation("GetCountryByName method called in CountriesRepo");

            return await _db.Countries.FirstOrDefaultAsync(c => c.CountryName == CountryName);
        }
    }
}
