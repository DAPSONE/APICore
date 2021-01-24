using System.Collections.Generic;
using System;

namespace JWTCore.Authentication.Models
{
    public class TokenDescription
    {
        public string Value { get; set; }
        public DateTime Now { get; set; }
        public DateTime Expired { get; set; }
    }
}