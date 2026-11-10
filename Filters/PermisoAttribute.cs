using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FloresMvc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermisoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _clave;

        public PermisoAttribute(string clave)
        {
            _clave = clave;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            if (!user.HasClaim("permiso", _clave))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
