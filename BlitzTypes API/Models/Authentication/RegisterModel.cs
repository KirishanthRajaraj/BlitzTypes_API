﻿using System.ComponentModel.DataAnnotations;

namespace BlitzTypes_API.Models.Authentication
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
