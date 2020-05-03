﻿﻿using System.ComponentModel.DataAnnotations;

namespace Cycler.Controllers.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        
        public string TimeOffset { get; set; }
    }
}