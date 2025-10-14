using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTOs.PersonDTOs
{
    public class PersonUpdateRequest
    {
        [Required]
        public Guid PersonID { get; set; }

        [Required(ErrorMessage = "Person Name cannot be blank")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Person Email cannot be blank")]
        [EmailAddress]
        public string? Email { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        [Required]
        public Guid? CountryID { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        /// <summary>
        /// Converts PersonUpdateRequest to Person entity
        /// </summary>
        /// <param name="personRequest"></param>
        public Person ToPerson()
        {
            return new Person()
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
