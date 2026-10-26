using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Fincas
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdFinca { get; set; }
        [Display(Name = "Nombre Finca")]
        public string NomFinca { get; set; }
        public DateTime fecha_Creacion { get; set; }

        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}