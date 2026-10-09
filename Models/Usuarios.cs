using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Usuarios
    {
        [Key]
        [Display(Name ="Usuario")]
        public int IdUsuarios {get; set;}
        [Display(Name ="Rol")] 
        public int IdRol {get; set;}
        [Display(Name ="Nombre")]
        public string Nombre {get; set;}
        [Display(Name ="Apellido")]
        public string Apellido {get; set;}
        [Display(Name ="Correo Electronico")]
        public string email {get; set;}
        [Display(Name ="Fecha_Creacion")]
        public DateTime fecha_creacion {get; set;}
    
    }
}
    
