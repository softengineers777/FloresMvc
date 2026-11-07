using System;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Models;

namespace FloresMvc.Models
{
    public class Sucursal
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdSucursal { get; set; }
        [Required]
        [Display(Name = "Nombre Sucursal")]
        public required string NombreSuc { get; set; }
        public DateTime fecha_Creacion {get; set;}

        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}