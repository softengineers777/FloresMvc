using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Roles
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdRol { get; set; }
        [Required]
        [Display(Name = "Nombre")]
        public required string Nombre { get; set; }
        public DateTime fecha_Creacion { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}
