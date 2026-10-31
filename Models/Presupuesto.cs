using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using FloresMvc.Models;
using Microsoft.AspNetCore.Mvc;


namespace FloresMvc.Models
{
    public class Presupuesto
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdPresupuesto { get; set; }
        [Display(Name ="Area")]
        public int IdArea { get; set; }
        [Display(Name ="Finca")]
        public int IdFinca { get; set; }
        [Display(Name ="Categoria")]
        public int IdCategoria { get; set; }
        [Display(Name ="SubCategoria")]
        public int IdSubcategoria { get; set; }
        public DateTime fecha_Creacion { get; set; }
        [Required]
        [Display(Name ="Nombre")]
        public required string Nombre { get; set; }
        [Required]
        [Display(Name ="Apellido")]
        public required string Apellido { get; set; }
        [Required]
        [Display(Name ="Cedula")]
        public required string Cedula { get; set; }
        public Double Subtotal { get; set; }
        public Double Total { get; set; }
        [Required]
        [Display(Name ="Observaciones")]
        public required string Observaciones { get; set; }
        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }


    }
}