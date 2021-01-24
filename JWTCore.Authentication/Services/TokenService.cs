using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using JWTCore.Authentication.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using JWTCore.Authentication.Base;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace JWTCore.Authentication.Services
{
    public static class TokenService
    {
        public static JwtBearerOptions DefaultJwtBearerOptions(this JwtBearerOptions options, AppSettings settings)
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                RequireSignedTokens = false,
                ClockSkew = TimeSpan.Zero,
                TokenDecryptionKey = new X509SecurityKey(new X509Certificate2(settings.PrivateKey, settings.PasswordCertificate))
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 401;
                    var content = new
                    {
                        error = context.Error,
                        description = context.ErrorDescription,
                        statusCode = context.Response.StatusCode
                    };
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(content));
                },
                OnTokenValidated = context => 
                {
                    var claim = context.Principal.Claims.SingleOrDefault(x => x.Type == "id");
                    if (claim is null)
                        return Task.CompletedTask;
                    var id = Convert.ToInt32(claim.Value);
                    var token = (context.SecurityToken as JwtSecurityToken).RawData;

                    var tokens = TokenDictionary.GetTokens(id);
                    if (tokens is null)
                        return Task.CompletedTask;

                    if (!tokens.Any(x => x.Value.Equals(token, StringComparison.OrdinalIgnoreCase)))
                        context.Fail("Token invalid");

                    return Task.CompletedTask;
                }
            };
            return options;
        }
    }
}