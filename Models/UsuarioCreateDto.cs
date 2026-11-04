using System;

namespace FloresMvc.Models
{
    /// <summary>
    /// Datos que envía el modal de creación de usuarios. Incluye la contraseña,
    /// que no forma parte de la entidad Usuarios sino de la tabla login.
    /// </summary>
    public class UsuarioCreateDto
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public DateTime? fecha_creacion { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; }
    }
}
