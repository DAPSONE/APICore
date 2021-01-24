using System.ComponentModel.DataAnnotations;
using System;

namespace JWTCore.Authentication.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}