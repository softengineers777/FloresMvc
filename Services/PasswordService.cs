using FloresMvc.Models;
using Microsoft.AspNetCore.Identity;

namespace FloresMvc.Services
{
    /// <summary>
    /// Centraliza el hash y la verificación de contraseñas.
    /// Usa el PasswordHasher de ASP.NET Core (PBKDF2 + sal aleatoria), de modo
    /// que en la tabla login nunca se guarda la contraseña en texto plano.
    /// </summary>
    public class PasswordService
    {
        private readonly IPasswordHasher<Login> _hasher;

        // El hasher de ASP.NET Core no usa los datos del usuario para generar
        // el hash, así que basta con una instancia de apoyo.
        private static readonly Login UsuarioApoyo = new() { password_hash = string.Empty };

        public PasswordService(IPasswordHasher<Login> hasher)
        {
            _hasher = hasher;
        }

        /// <summary>Genera el hash seguro de una contraseña.</summary>
        public string Hash(string password)
        {
            return _hasher.HashPassword(UsuarioApoyo, password);
        }

        /// <summary>
        /// Verifica una contraseña contra el valor almacenado.
        /// Devuelve true si coincide. Cuando el valor guardado es antiguo
        /// (texto plano o un formato obsoleto), <paramref name="requiereRehash"/>
        /// queda en true para que el llamador lo reemplace por un hash nuevo.
        /// </summary>
        public bool Verify(Login credencial, string password, out bool requiereRehash)
        {
            requiereRehash = false;

            var almacenado = credencial.password_hash;
            if (string.IsNullOrEmpty(almacenado))
            {
                return false;
            }

            try
            {
                var resultado = _hasher.VerifyHashedPassword(credencial, almacenado, password);

                if (resultado != PasswordVerificationResult.Failed)
                {
                    requiereRehash = resultado == PasswordVerificationResult.SuccessRehashNeeded;
                    return true;
                }
            }
            catch (FormatException)
            {
                // El valor guardado no tiene formato de hash (probablemente es
                // una contraseña vieja en texto plano). Se evalúa abajo.
            }

            // Compatibilidad con contraseñas viejas guardadas en texto plano:
            // si coinciden, se acepta el acceso y se marca para hashear.
            if (almacenado == password)
            {
                requiereRehash = true;
                return true;
            }

            return false;
        }
    }
}
