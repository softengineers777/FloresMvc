using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;

namespace FloresMvc.Models
{
    public class Colaborador
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdColaborador { get; set; }
        [Required]
        [Display(Name = "Cargo")]
        public int IdCargo {get; set;}
        [Required]
        [Display(Name = "Nombre")]
        public required string Nombre { get; set; }
        [Required]
        [Display(Name = "Apellido")]
        public required string Apellido { get; set; }
        [Required]
        [Display(Name = "Codigo Empleado")]
        public required string CodigoEmp { get; set; }
        [Required]
        [Display(Name = "Cedula")]
        public required string Cedula { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }



    }
}