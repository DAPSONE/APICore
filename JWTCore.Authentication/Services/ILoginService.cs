using JWTCore.Authentication.Models;

namespace JWTCore.Authentication.Services
{
    public interface ILoginService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        AuthenticateResponse Refresh(int id, string currentToken);
        bool Revoke(int id, string currentToken);
    }
}