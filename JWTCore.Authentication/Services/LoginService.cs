using JWTCore.Authentication.Models;
using System.Collections.Generic;
using JWTCore.Authentication.Entities;
using JWTCore.Authentication.Helpers;
using Microsoft.Extensions.Options;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using JWTCore.Authentication.Base;

namespace JWTCore.Authentication.Services
{
    public class LoginService : ILoginService
    {
        private UserContext _context;
        private readonly AppSettings _appSettings;

        public LoginService(UserContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);
            if (user is null)
                return null;

            user.UsersModules = _context.UsersModules
            .Where(x => x.IdUser == user.Id)
            .Join(_context.Modules, x => x.IdModule, y => y.Id, (x, y) => new UsersModules
            {
                Id = x.Id,
                IdUser = user.Id,
                User = user,
                IdModule = y.Id,
                Module = y
            }).ToList();

            var token = GenerateToken(user);
            if (token is null)
                return null;

            return new AuthenticateResponse(user, token);
        }

        public AuthenticateResponse Refresh(int id, string currentToken)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == id);
            if (user is null)
                return null;

            user.UsersModules = _context.UsersModules
            .Where(x => x.IdUser == user.Id)
            .Join(_context.Modules, x => x.IdModule, y => y.Id, (x, y) => new UsersModules
            {
                Id = x.Id,
                IdUser = user.Id,
                User = user,
                IdModule = y.Id,
                Module = y
            }).ToList();

            TokenDictionary.Remove(id, currentToken);
            var token = GenerateToken(user);
            if (token is null)
                return null;

            return new AuthenticateResponse(user, token);
        }

        public bool Revoke(int id, string currentToken)
        {
            var user = _context.Users.SingleOrDefault(x => x.Id == id);
            if (user is null)
                return false;
            TokenDictionary.Remove(id, currentToken);
            return true;
        }

        private TokenDescription GenerateToken(User user)
        {
            var tokenHandle = new JwtSecurityTokenHandler();
            var date = DateTime.UtcNow;
            var expired = date.AddMinutes(20);

            var modules = new List<Claim>();
            modules.Add(new Claim("id", user.Id.ToString()));
            modules.Add(new Claim("username", user.Username));
            foreach (var module in user.UsersModules)
            {
                modules.Add(new Claim("module", module.Module.Module));
            }
            try
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(modules),
                    Expires = expired,
                    Audience = _appSettings.Audience,
                    Issuer = _appSettings.Issuer,
                    EncryptingCredentials = new X509EncryptingCredentials(new X509Certificate2(_appSettings.PublicKey)),
                };
                var token = new TokenDescription
                {
                    Value = tokenHandle.CreateEncodedJwt(tokenDescriptor),
                    Now = date,
                    Expired = expired
                };
                TokenDictionary.Add(user.Id, token);
                return token;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}