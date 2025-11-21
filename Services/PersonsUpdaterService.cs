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
    public class PersonsUpdaterService : IPersonsUpdaterService
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

        public PersonsUpdaterService(IPersonsRepository personRepo, ILogger<PersonsGetterService> logger)
        {
            _personRepo = personRepo;
            _logger = logger;
        }
        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            _logger.LogInformation("UpdatePerson method called in PersonsService");

            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest), "PersonUpdateRequest cannot be null");

            ValidationHelper.ModelValidation(personUpdateRequest);

            Person? existingPerson = await _personRepo.GetPersonById(personUpdateRequest.PersonID);

            if (existingPerson == null)
                throw new ArgumentException($"Person with ID {personUpdateRequest.PersonID} not found");

            existingPerson.Name = personUpdateRequest.Name;
            existingPerson.Email = personUpdateRequest.Email;
            existingPerson.Gender = personUpdateRequest.Gender;
            existingPerson.Address = personUpdateRequest.Address;
            existingPerson.CountryID = personUpdateRequest.CountryID;
            existingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            existingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personRepo.UpdatePerson(existingPerson);

            return await PersonToPersonResponseAsync(existingPerson);
        }
    }
}
