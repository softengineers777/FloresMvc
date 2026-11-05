using System.Security.Claims;

namespace FloresMvc.Extensions
{
    /// <summary>
    /// Utilidades para obtener los datos del usuario autenticado que se
    /// guardan en las columnas de auditoría (Owner / LastModify).
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Devuelve el nombre del usuario que inició sesión. Si por alguna
        /// razón el claim de nombre no existe, usa el email y, como último
        /// recurso, "Sistema".
        /// </summary>
        public static string ObtenerNombreUsuario(this ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return "Sistema";
            }

            var nombre = user.Identity.Name;
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }

            var email = user.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            return "Sistema";
        }
    }
}
