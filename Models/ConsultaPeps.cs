using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Models;
using System.ComponentModel.DataAnnotations;


namespace FloresMvc.Models
{
    public class ConsultaPeps
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdConsulta { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int IdUsuario { get; set; }
        [Required]
        [Display(Name = "Cedula")]
        public string? Cedula { get; set; }

        [Display(Name = "Cargo")]
        public int IdCargo { get; set; }

        [Display(Name = "Area")]
        public int IdArea { get; set; }

        [Display(Name = "¿Usted o un familiar es PEP?")]
        public bool? EsPep { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public string? Firma { get; set; }

        [MaxLength(30)]
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }

        [MaxLength(30)]
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }
    }
}