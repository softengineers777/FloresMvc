using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Login
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdLogin { get; set; }
        [Display(Name = "usuario")]
        public int user_id { get; set; }
        [Required]
        [Display(Name = "Contraseña")]
        public required string password_hash { get; set; }
        [Display(Name = "ultimo_acceso")]
        public DateTime ultimo_acceso { get; set; }
        [Display(Name = "Activo")]
        public Boolean activo { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creós
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}
 
