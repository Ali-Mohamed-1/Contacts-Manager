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
    public class PersonsAdderService : IPersonsAdderService
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

        public PersonsAdderService(IPersonsRepository personRepo, ILogger<PersonsGetterService> logger)
        {
            _personRepo = personRepo;
            _logger = logger;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            _logger.LogInformation("AddPerson method called in PersonsService");

            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest), "PersonAddRequest cannot be null");

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            await _personRepo.AddPerson(person);

            return await PersonToPersonResponseAsync(person);
        }
    }
}
