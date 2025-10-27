using Entities;
using Entities.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepo : IPersonsRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PersonsRepo> _logger;

        public PersonsRepo(AppDbContext db, ILogger<PersonsRepo> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _logger.LogInformation("AddPerson method called in PersonsRepo");

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeletePersonByID(Guid personID)
        {
            _logger.LogInformation("DeletePersonByID method called in PersonsRepo");

            _db.Persons.RemoveRange(_db.Persons.Where(temp => temp.PersonID == personID));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method called in PersonsRepo");

            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            _logger.LogInformation("GetFilteredPersons method called in PersonsRepo");

            return await _db.Persons.Include("Country")
             .Where(predicate)
             .ToListAsync();
        }

        public async Task<Person?> GetPersonById(Guid PersonId)
        {
            _logger.LogInformation("GetPersonById method called in PersonsRepo");

            return await _db.Persons.Include("Country")
             .FirstOrDefaultAsync(temp => temp.PersonID == PersonId);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            _logger.LogInformation("UpdatePerson method called in PersonsRepo");

            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == person.PersonID);

            if (matchingPerson == null)
                return person;

            matchingPerson.Name = person.Name;
            matchingPerson.Email = person.Email;
            matchingPerson.DateOfBirth = person.DateOfBirth;
            matchingPerson.Gender = person.Gender;
            matchingPerson.CountryID = person.CountryID;
            matchingPerson.Address = person.Address;
            matchingPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;

            int countUpdated = await _db.SaveChangesAsync();

            return matchingPerson;
        }
    }
}
