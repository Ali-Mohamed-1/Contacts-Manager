using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs.PersonDTOs
{
    public class PersonAddRequest
    {
        [Required(ErrorMessage = "Person Name cannot be blank.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Person Email cannot be blank.")]
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "The Country field is required")]
        public Guid? CountryID { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        /// <summary>
        /// Converts PersonAddRequest to Person entity
        /// </summary>
        /// <param name="personRequest"></param>
        public Person ToPerson()
        {
            return new Person()
            {
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
