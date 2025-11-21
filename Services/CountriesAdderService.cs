using System;
using System.Linq;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;

namespace Services
{
	public class CountriesAdderService : ICountriesAdderService
	{
		private readonly ICountriesRepository _countriesRepo;
		private readonly ILogger<CountriesAdderService> _logger;

		public CountriesAdderService(ICountriesRepository countriesRepo, ILogger<CountriesAdderService> logger)
		{
			_countriesRepo = countriesRepo;
			_logger = logger;
		}

		public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
		{
			_logger.LogInformation("AddCountry method called in CountriesAdderService");

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
	}
}

