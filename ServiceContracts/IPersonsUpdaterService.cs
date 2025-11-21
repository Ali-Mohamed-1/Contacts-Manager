using System;
using ServiceContracts.DTOs.PersonDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents buisness logic for Person entity
    /// </summary>
    public interface IPersonsUpdaterService
    {
        /// <summary>
        /// Updates an existing person
        /// </summary>
        /// <param name="personUpdateRequest"></param>
        /// <returns></returns>
        public Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);
    }
}
