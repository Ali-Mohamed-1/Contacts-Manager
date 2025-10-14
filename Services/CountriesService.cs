using System;
using System.Linq;
using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;

namespace Services
{
	public class CountriesService : ICountryService
	{
		private readonly AppDbContext _db;
		public CountriesService(AppDbContext personsDbContext)
		{
			_db = personsDbContext;
		}

		public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
		{
			if (countryAddRequest == null)
				throw new ArgumentNullException(nameof(countryAddRequest));

			if (string.IsNullOrWhiteSpace(countryAddRequest.CountryName))
				throw new ArgumentException("CountryName is required", nameof(countryAddRequest));

			if (_db.Countries.Any(c => string.Equals(c.CountryName, countryAddRequest.CountryName, StringComparison.Ordinal)))
				throw new ArgumentException("Country name already exists", nameof(countryAddRequest));

			Country country = countryAddRequest.ToCountry();
			country.CountryID = Guid.NewGuid();

			_db.Countries.Add(country);
			await _db.SaveChangesAsync();

            return country.ToCountryResponse();
		}

        public async Task<List<CountryResponse>> GetAllCountries()
        {
			return await _db.Countries
                .Select(country => country.ToCountryResponse()).ToListAsync();
        }

		public async Task<CountryResponse>? GetCountryByID(Guid? countryID)
		{
			if (countryID == null)
				return null;

			Country? country = await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryID);
			
			if (country == null)
                return null;

            return country.ToCountryResponse();
        }
    }
}