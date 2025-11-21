using System;
using ServiceContracts.DTOs.PersonDTOs;

namespace ServiceContracts
{
    /// <summary>
    /// Represents buisness logic for Person entity
    /// </summary>
    public interface IPersonsSorterService
    {
        /// <summary>
        /// Returns sorted persons by any field
        /// </summary>
        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, bool isAscending);
    }
}
