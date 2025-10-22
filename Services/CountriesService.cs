using System;
using System.Linq;
using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;

namespace Services
{
	public class CountriesService : ICountryService
	{
		private readonly ICountriesRepository _countriesRepo;
		public CountriesService(ICountriesRepository countriesRepo)
		{
			_countriesRepo = countriesRepo;
		}

		public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
		{
			if (countryAddRequest == null)
				throw new ArgumentNullException(nameof(countryAddRequest));

			if (string.IsNullOrWhiteSpace(countryAddRequest.CountryName))
				throw new ArgumentException("CountryName is required", nameof(countryAddRequest));

			var existingCountry = await _countriesRepo.GetCountryByName(countryAddRequest.CountryName);
			if (existingCountry != null)
				throw new ArgumentException("Country name already exists", nameof(countryAddRequest));

			Country country = countryAddRequest.ToCountry();
			country.CountryID = Guid.NewGuid();

			await _countriesRepo.AddCountry(country);

            return country.ToCountryResponse();
		}

        public async Task<List<CountryResponse>> GetAllCountries()
        {
			return (await _countriesRepo.GetAllCountries())
                .Select(country => country.ToCountryResponse()).ToList();
        }

		public async Task<CountryResponse>? GetCountryByID(Guid? countryID)
		{
			if (countryID == null)
				return null;

			Country? country = await _countriesRepo.GetCountryByID(countryID.Value);

            if (country == null)
                return null;

            return country.ToCountryResponse();
        }
    }
}