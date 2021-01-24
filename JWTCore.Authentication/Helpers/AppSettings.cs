using Microsoft.EntityFrameworkCore;
using JWTCore.Authentication.Entities;

namespace JWTCore.Authentication.Helpers
{
    public class AppSettings
    {
        public string PasswordCertificate { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}