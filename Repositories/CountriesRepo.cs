using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class CountriesRepo : ICountriesRepository
    {
        private readonly AppDbContext _db;

        public CountriesRepo(AppDbContext db)
        {
            _db = db;
        }


        public async Task<Country> AddCountry(Country country)
        {
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            return country;
        }

        public async Task<List<Country>> GetAllCountries()
        {
            return await _db.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryByID(Guid countryID)
        {
            return await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryID);
        }

        public async Task<Country?> GetCountryByName(string CountryName)
        {
            return await _db.Countries.FirstOrDefaultAsync(c => c.CountryName == CountryName);
        }
    }
}
