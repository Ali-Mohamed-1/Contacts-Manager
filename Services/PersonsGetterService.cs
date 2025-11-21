using Entities;
using ServiceContracts;
using ServiceContracts.DTOs.PersonDTOs;
using Services.Helpers;
using Xunit.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class PersonsGetterService : IPersonsGetterService
    {
        private readonly IPersonsRepository _personRepo;
        private readonly ILogger<PersonsGetterService> _logger;

        private async Task<PersonResponse> PersonToPersonResponseAsync(Person person)
        {
            _logger.LogInformation("PersonToPersonResponseAsync method called in PersonsService");

            PersonResponse personResponse = person.ToPersonResponse();

            personResponse.CountryName = person.Country?.CountryName;

            return personResponse;
        }

        public PersonsGetterService(IPersonsRepository personRepo, ILogger<PersonsGetterService> logger)
        {
            _personRepo = personRepo;
            _logger = logger;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method called in PersonsService");

            var persons = await _personRepo.GetAllPersons();

            return persons.Select(person =>
            {
                var response = person.ToPersonResponse();
                response.CountryName = person.Country?.CountryName;
                return response;
            }).ToList();
        }

        public async Task<PersonResponse?> GetPersonByID(Guid? personID)
        {
            _logger.LogInformation("GetPersonByID method called in PersonsService");

            if (personID == null)
                return null;

            Person? person = await _personRepo.GetPersonById(personID.Value);
            if (person == null)
                return null;

            return await PersonToPersonResponseAsync(person);
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons method called in PersonsService");

            List<Person> persons = searchBy switch
            {
                nameof(PersonResponse.Name) =>
                    await _personRepo.GetFilteredPersons(person =>
                        person.Name.Contains(searchString)),

                nameof(PersonResponse.Email) =>
                    await _personRepo.GetFilteredPersons(person =>
                        person.Email.Contains(searchString)),

                nameof(PersonResponse.Address) =>
                    await _personRepo.GetFilteredPersons(person =>
                        person.Address.Contains(searchString)),

                nameof(PersonResponse.Gender) =>
                    await _personRepo.GetFilteredPersons(person =>
                        person.Gender.Equals(searchString)),

                nameof(PersonResponse.CountryName) =>
                        await _personRepo.GetFilteredPersons(person =>
                            !string.IsNullOrEmpty(person.Country.CountryName) &&
                            person.Country.CountryName.Contains(searchString)),

                _ => await _personRepo.GetAllPersons()
            };

            return persons.Select(person =>
            {
                var response = person.ToPersonResponse();
                response.CountryName = person.Country?.CountryName;
                return response;
            }).ToList();
        }
    }
}
