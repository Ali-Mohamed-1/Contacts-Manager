using System;
using Entities;
using ServiceContracts.DTOs.CountryDTOs;

namespace ServiceContracts.DTOs.CountryDTOs
{
	/// <summary>
	/// DTO class to use as return type for most of CountriesServices methods
	/// </summary>
	public class CountryResponse
	{
		public Guid CountryID { get; set; }
		public string? CountryName { get; set; }

	}
}

public static class CountryResponseExtensions
{
	public static CountryResponse ToCountryResponse(this Country country)
	{
		return new CountryResponse
		{
			CountryID = country.CountryID,
			CountryName = country.CountryName
		};
	}
}