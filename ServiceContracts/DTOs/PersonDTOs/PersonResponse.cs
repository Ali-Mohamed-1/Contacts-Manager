using Entities;
using ServiceContracts.DTOs.PersonDTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.PersonDTOs
{
    public class PersonResponse
    {
        public Guid PersonID { get; set; }
        public string? Name { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public Guid? CountryID { get; set; }
        public string? CountryName { get; set; }
        public double? Age { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        public PersonUpdateRequest ToUpdateRequest()
        {
            return new PersonUpdateRequest
            {
                PersonID = this.PersonID,
                Name = this.Name,
                Email = this.Email,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                Address = this.Address,
                CountryID = this.CountryID,
                ReceiveNewsLetters = this.ReceiveNewsLetters
            };
        }
    }
}

public static class PersonExtensions
{
    /// <summary>
    /// Converts Person entity to PersonResponse DTO
    /// </summary>
    /// <param name="person"></param>
    /// <returns></returns>
    public static PersonResponse ToPersonResponse(this Person person)
    {
        return new PersonResponse()
        {
            PersonID = person.PersonID,
            Name = person.Name,
            Email = person.Email,
            DateOfBirth = person.DateOfBirth,
            Gender = person.Gender,
            Address = person.Address,
            CountryID = person.CountryID,
            ReceiveNewsLetters = person.ReceiveNewsLetters,
            Age = (person.DateOfBirth != null)? Math.Round((DateTime.Now - person.DateOfBirth).TotalDays / 365.25, 1) : null
        };
    }
}