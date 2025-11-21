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
    public class PersonsSorterService : IPersonsSorterService
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

        public PersonsSorterService(IPersonsRepository personRepo, ILogger<PersonsGetterService> logger)
        {
            _personRepo = personRepo;
            _logger = logger;
        }

        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, bool isAscending)
        {
            _logger.LogInformation("GetSortedPersons method called in PersonsService");

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
    }
}
