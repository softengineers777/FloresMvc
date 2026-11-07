using System;
using System.Data;
using System.ComponentModel.DataAnnotations;
using FloresMvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FloresMvc.Models
{
    public class Cargo
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdCargo { get; set; }
        [Required]
        [Display(Name = "Nombre Cargo")]
        public required string NomCargo { get; set; }
        public DateTime fecha_Creacion {get; set;}
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}