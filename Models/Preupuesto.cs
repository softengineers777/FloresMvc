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
        public int IdArea { get; set; }
        public int IdFinca { get; set; }
        public int IdCategoria { get; set; }
        public int IdSubcategoria { get; set; }
        public DateTime fecha_Creacion { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cedula { get; set; }
        public Double Subtotal { get; set; }
        public Double Total { get; set; }
        public string Observaciones { get; set; }
        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }


    }
}