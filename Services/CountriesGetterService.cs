using System;
using System.Linq;
using Entities;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTOs.CountryDTOs;

namespace Services
{
	public class CountriesGetterService : ICountriesGetterService
	{
		private readonly ICountriesRepository _countriesRepo;
		private readonly ILogger<CountriesGetterService> _logger;

		public CountriesGetterService(ICountriesRepository countriesRepo, ILogger<CountriesGetterService> logger)
		{
			_countriesRepo = countriesRepo;
			_logger = logger;
		}

		public async Task<List<CountryResponse>> GetAllCountries()
		{
			_logger.LogInformation("GetAllCountries method called in CountriesGetterService");

			return (await _countriesRepo.GetAllCountries())
				.Select(country => country.ToCountryResponse()).ToList();
		}

		public async Task<CountryResponse>? GetCountryByID(Guid? countryID)
		{
			_logger.LogInformation("GetCountryByID method called in CountriesGetterService");

			if (countryID == null)
				return null;

			Country? country = await _countriesRepo.GetCountryByID(countryID.Value);

			if (country == null)
				return null;

			return country.ToCountryResponse();
		}
	}
}

