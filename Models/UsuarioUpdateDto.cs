using System;

namespace FloresMvc.Models
{
    /// <summary>
    /// Datos que se pueden editar directamente desde la tabla (Tabulator).
    /// Se usa un DTO para no exponer ni deserializar la entidad completa:
    /// solo se actualizan nombre, apellido y correo.
    /// </summary>
    public class UsuarioUpdateDto
    {
        public int IdUsuarios { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }
}
