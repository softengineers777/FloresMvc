using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Login
    {
        [Key]
        [Display(Name ="Consecutivo")]
        public int IdLogin {get; set;}
        [Display(Name = "usuario")]
        public int user_id {get; set;}
        [Display(Name ="Contraseña")]
        public string password_hash {get;set;}
        [Display(Name = "ultimo_acceso")] 
        public DateTime ultimo_acceso {get;set;}
        [Display(Name = "Activo")]
        public Boolean activo {get; set;}
    }
}    
