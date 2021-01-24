using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JWTCore.Authentication.Attributes
{
    public class CAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private IEnumerable<string> _modules;
        public string Module { get; set; }

        public CAuthorizeAttribute()
        {
            _modules = Enumerable.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var filters = context.ActionDescriptor as ControllerActionDescriptor;
            var isValidMethod = filters.MethodInfo.GetCustomAttributes(inherit: true).Any(x => x.GetType().Equals(typeof(AllowAnonymousAttribute)));
            var isValidController = filters.ControllerTypeInfo.GetCustomAttributes(inherit: true).Any(x => x.GetType().Equals(typeof(AllowAnonymousAttribute)));
            if (isValidMethod || isValidController)
                return;

            var claims = context.HttpContext.User.Claims;
            InitModules();
            if (!claims.Any(x => x.Type == "module" && _modules.Any(y => x.Value.Equals(y.Trim(), System.StringComparison.OrdinalIgnoreCase))))
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult("No tiene permisos");
            }
        }

        private void InitModules()
        {
            if (!string.IsNullOrEmpty(Module))
            {
                _modules = Module.Split(',');
            }
        }
    }
}