using System;
using System.Data;
using FloresMvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Numerics;


namespace FloresMvc.Models
{
    public class ModeloDisciplinario
    {
        [Key]
        [Display(Name = "Consecutivo")]
        [Required]
        public int IdModelo { get; set; }
        [Required]
        [Display(Name = "Area")]
        public int IdArea { get; set; }
        [Required]
        [Display(Name = "Fecha de Creacion")]
        public DateTime fecha_creacion { get; set; }
        [Required]
        [Display(Name = "Para")]
        public required string para { get; set; }
        [Required]
        [Display(Name = "Asunto")]
        public required string asunto { get; set; }

        [Required]
        [Display(Name = "Nombre del Trabajador")]
        public required string NomTrabajador { get; set; }

        [Required]
        [Display(Name = "Cargo")]
        public int cargo { get; set; }

        [Required]
        [Display(Name = "Proceso")]
        public required string Proceso { get; set; }

        [Required]
        [Display(Name = "Situacion")]
        public required string Situacion { get; set; }

        [Required]
        [Display(Name = "Firma")]
        public required string Firma { get; set; }
        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }



    }


}