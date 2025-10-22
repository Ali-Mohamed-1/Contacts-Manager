using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    public interface IPersonsRepository
    {
        Task<Person> AddPerson(Person person);

        Task<List<Person>> GetAllPersons();

        Task<Person?> GetPersonById(Guid PersonId);

        Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);

        Task<bool> DeletePersonByID(Guid personID);

        Task<Person> UpdatePerson(Person person);
    }
}
