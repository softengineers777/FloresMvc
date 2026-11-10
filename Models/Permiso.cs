using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Permiso
    {
        [Key]
        public int IdPermiso { get; set; }

        [Required]
        [MaxLength(100)]
        public string Clave { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Modulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}
