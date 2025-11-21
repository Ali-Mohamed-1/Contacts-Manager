using System;
using ServiceContracts.DTOs.PersonDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents buisness logic for Person entity
    /// </summary>
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Adds a new person to the persons list
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>The same person details along with the newly generated PersonID</returns>
        public Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);
    }
}
