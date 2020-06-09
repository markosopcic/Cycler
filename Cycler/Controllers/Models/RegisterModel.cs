﻿using System.ComponentModel;
 using System.ComponentModel.DataAnnotations;

namespace Cycler.Controllers.Models
{
    public class RegisterModel
    {
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        [DisplayName("Confirm Password")]
        [MinLength(6)]
        public string ConfirmPassword { get; set; }
    }
}