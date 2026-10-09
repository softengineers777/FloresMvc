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
    }
}