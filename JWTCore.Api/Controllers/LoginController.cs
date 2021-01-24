using Microsoft.AspNetCore.Mvc;
using JWTCore.Authentication.Models;
using JWTCore.Authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using JWTCore.Authentication.Attributes;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace JWTCore.Api.Controllers
{
    [ApiController]
    [CAuthorize]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(HttpContext.Request.Path);
        }

        [AllowAnonymous]
        [HttpPost("auth")]
        public IActionResult Auth([FromBody] AuthenticateRequest model)
        {
            System.Threading.Thread.Sleep(5000);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = _loginService.Authenticate(model);
            if (response is null)
                return BadRequest(new { message = "Username or Password is incorrect" });

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var id = Convert.ToInt32(HttpContext.User.Claims.SingleOrDefault(y => y.Type == "id").Value);
            var response = _loginService.Refresh(id, await HttpContext.GetTokenAsync("access_token"));
            if (response is null)
                return BadRequest(new { message = "Error to validate" });
            return Ok(response);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var id = Convert.ToInt32(HttpContext.User.Claims.SingleOrDefault(y => y.Type == "id").Value);
            var response = _loginService.Revoke(id, await HttpContext.GetTokenAsync("access_token"));
            if (!response)
                return NotFound(new { message = "Token not found" });
            return Ok(new { message = "Logout" });
        }
    }
}