using Entities;
using ServiceContracts;
using ServiceContracts.DTOs.PersonDTOs;
using Services.Helpers;
using Xunit.Abstractions;
using System;
using System.ComponentModel.DataAnnotations;
using Entities.Data;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly AppDbContext _db;
        private readonly ICountryService _countryService;


        private async Task<PersonResponse> PersonToPersonResponseAsync(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();
            var country = await _countryService.GetCountryByID(person.CountryID);
            personResponse.CountryName = country?.CountryName;

            return personResponse;
        }

        public PersonsService(AppDbContext personsDbContext, ICountryService countryService)
        {
            _countryService = countryService;
            _db = personsDbContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest), "PersonAddRequest cannot be null");

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return await PersonToPersonResponseAsync(person);
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            var persons = await _db.Persons.ToListAsync();
            var responses = new List<PersonResponse>(persons.Count);
            foreach (var person in persons)
            {
                responses.Add(await PersonToPersonResponseAsync(person));
            }
            return responses;
        }

        public async Task<PersonResponse?> GetPersonByID(Guid? personID)
        {
            if (personID == null)
                return null;

            Person? person = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);
            if (person == null)
                return null;

            return await PersonToPersonResponseAsync(person);
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            var persons = await _db.Persons.ToListAsync();
            var allPersons = new List<PersonResponse>(persons.Count);
            foreach (var person in persons)
            {
                allPersons.Add(await PersonToPersonResponseAsync(person));
            }
            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrEmpty(searchBy) || string.IsNullOrEmpty(searchString))
                return matchingPersons;

            switch (searchBy)
            {
                case nameof(PersonResponse.Name):
                    matchingPersons = allPersons.Where(person =>
                        !string.IsNullOrEmpty(person.Name)
                        && person.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(PersonResponse.Email):
                    matchingPersons = allPersons.Where(person =>
                        !string.IsNullOrEmpty(person.Email)
                        && person.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(PersonResponse.Address):
                    matchingPersons = allPersons.Where(person =>
                        !string.IsNullOrEmpty(person.Address)
                        && person.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(PersonResponse.Gender):
                    matchingPersons = allPersons.Where(person =>
                        !string.IsNullOrEmpty(person.Gender)
                        && person.Gender.Equals(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;

                case nameof(PersonResponse.CountryName):
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        matchingPersons = allPersons.Where(person =>
                            !string.IsNullOrEmpty(person.CountryName) &&
                            person.CountryName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        matchingPersons = new List<PersonResponse>();
                    }
                    break;

                default:
                    throw new ArgumentException($"Invalid search criteria: {searchBy}");
            }

            return matchingPersons;
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

            Person? existingPerson = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personUpdateRequest.PersonID);
            if (existingPerson == null)
                throw new ArgumentException($"Person with ID {personUpdateRequest.PersonID} not found");

            existingPerson.Name = personUpdateRequest.Name;
            existingPerson.Email = personUpdateRequest.Email;
            existingPerson.Gender = personUpdateRequest.Gender;
            existingPerson.Address = personUpdateRequest.Address;
            existingPerson.CountryID = personUpdateRequest.CountryID;
            existingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            existingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _db.SaveChangesAsync();

            return await PersonToPersonResponseAsync(existingPerson);
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if(personID == null)
                return false;

            Person? person = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);
            if (person == null)
                return false;

            _db.Persons.Remove(person);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
