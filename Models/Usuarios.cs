using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Usuarios
    {
        [Key]
        [Display(Name = "Usuario")]
        public int IdUsuarios { get; set; }
        [Display(Name = "Rol")]
        public int IdRol { get; set; }
        [Required]
        [Display(Name = "Nombre")]
        public  required string Nombre { get; set; }
        [Required]
        [Display(Name = "Apellido")]
        public required string Apellido { get; set; }
        [Required]
        [Display(Name = "Correo Electronico")]
        public required string email { get; set; }
        [Display(Name = "Fecha_Creacion")]
        public DateTime fecha_creacion { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }

}

    
