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

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly IPersonsRepository _personRepo;

        private async Task<PersonResponse> PersonToPersonResponseAsync(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();

            personResponse.CountryName = person.Country?.CountryName;

            return personResponse;
        }

        public PersonsService(IPersonsRepository personRepo, ICountryService countryService)
        {
            _personRepo = personRepo;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest), "PersonAddRequest cannot be null");

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            await _personRepo.AddPerson(person);

            return await PersonToPersonResponseAsync(person);
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            var persons = await _personRepo.GetAllPersons();

            return persons.Select(person => person.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonByID(Guid? personID)
        {
            if (personID == null)
                return null;

            Person? person = await _personRepo.GetPersonById(personID.Value);
            if (person == null)
                return null;

            return await PersonToPersonResponseAsync(person);
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
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

            return persons.Select(person => person.ToPersonResponse()).ToList();
        }

        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, bool isAscending)
        {
            List<PersonResponse> matchingPersons = allPersons;

            switch (sortBy)
            {
                case nameof(PersonResponse.Name):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.Name).ToList()
                        : allPersons.OrderByDescending(p => p.Name).ToList();
                    break;

                case nameof(PersonResponse.Email):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.Email).ToList()
                        : allPersons.OrderByDescending(p => p.Email).ToList();
                    break;

                case nameof(PersonResponse.DateOfBirth):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.DateOfBirth).ToList()
                        : allPersons.OrderByDescending(p => p.DateOfBirth).ToList();
                    break;

                case nameof(PersonResponse.Gender):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.Gender).ToList()
                        : allPersons.OrderByDescending(p => p.Gender).ToList();
                    break;

                case nameof(PersonResponse.Address):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.Address).ToList()
                        : allPersons.OrderByDescending(p => p.Address).ToList();
                    break;

                case nameof(PersonResponse.CountryName):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.CountryName).ToList()
                        : allPersons.OrderByDescending(p => p.CountryName).ToList();
                    break;

                case nameof(PersonResponse.ReceiveNewsLetters):
                    matchingPersons = isAscending
                        ? allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList()
                        : allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList();
                    break;

                default:
                    throw new ArgumentException($"Invalid sort criteria: {sortBy}");
            }

            return Task.FromResult(matchingPersons);
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
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

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if(personID == null)
                return false;

            Person? person = await _personRepo.GetPersonById(personID.Value);
            if (person == null)
                return false;

            await _personRepo.DeletePersonByID(personID.Value);

            return true;
        }
    }
}
