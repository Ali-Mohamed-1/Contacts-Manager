using System;
using ServiceContracts.DTOs.PersonDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents buisness logic for Person entity
    /// </summary>
    public interface IPersonsGetterService
    {
        /// <summary>
        /// Returns all persons from the persons list
        /// </summary>
        /// <returns>A list of PersonResponse objects</returns>
        public Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns a person by personID
        /// </summary>
        public Task<PersonResponse?> GetPersonByID(Guid? personID);

        /// <summary>
        /// Returns filtered persons by any search criteria
        /// </summary>
        public Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);
    }
}
