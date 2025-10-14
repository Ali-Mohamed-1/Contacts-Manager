using System;
using ServiceContracts.DTOs.PersonDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents buisness logic for Person entity
    /// </summary>
    public interface IPersonsService
    {
        /// <summary>
        /// Adds a new person to the persons list
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>The same person details along with the newly generated PersonID</returns>
        public Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

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

        /// <summary>
        /// Returns sorted persons by any field
        /// </summary>
        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, bool isAscending);

        /// <summary>
        /// Updates an existing person
        /// </summary>
        /// <param name="personUpdateRequest"></param>
        /// <returns></returns>
        public Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// Deletes an existing person
        /// </summary>
        public Task<bool> DeletePerson(Guid? personID);
    }
}
