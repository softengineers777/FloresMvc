using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Roles
    {
        [Key]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}
