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
    public class PersonsDeleterService : IPersonsDeleterService
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

        public PersonsDeleterService(IPersonsRepository personRepo, ILogger<PersonsGetterService> logger)
        {
            _personRepo = personRepo;
            _logger = logger;
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            _logger.LogInformation("DeletePerson method called in PersonsService");

            if (personID == null)
                return false;

            Person? person = await _personRepo.GetPersonById(personID.Value);
            if (person == null)
                return false;

            await _personRepo.DeletePersonByID(personID.Value);

            return true;
        }
    }
}
