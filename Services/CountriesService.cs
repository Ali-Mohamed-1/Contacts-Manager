using System;
using System.Linq;
using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;

namespace Services
{
	public class CountriesService : ICountryService
	{
		private readonly ICountriesRepository _countriesRepo;
		private readonly ILogger<CountriesService> _logger;
        private ICountriesRepository countriesRepo;

        public CountriesService(ICountriesRepository countriesRepo)
        {
            this.countriesRepo = countriesRepo;
        }

        public CountriesService(ICountriesRepository countriesRepo, ILogger<CountriesService> logger)
		{
			_countriesRepo = countriesRepo;
			_logger = logger;
		}

		public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
		{
			_logger.LogInformation("AddCountry method called in CountriesService");

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
			_logger.LogInformation("GetAllCountries method called in CountriesService");

			return (await _countriesRepo.GetAllCountries())
                .Select(country => country.ToCountryResponse()).ToList();
        }

		public async Task<CountryResponse>? GetCountryByID(Guid? countryID)
		{
			_logger.LogInformation("GetCountryByID method called in CountriesService");

			if (countryID == null)
				return null;

			Country? country = await _countriesRepo.GetCountryByID(countryID.Value);

            if (country == null)
                return null;

            return country.ToCountryResponse();
        }
    }
}