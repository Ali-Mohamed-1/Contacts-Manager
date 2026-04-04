using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTOs
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password can't be blank")]
        [DataType(DataType.EmailAddress)]
        public string? Password { get; set; } = string.Empty;
    }
}
