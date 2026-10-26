using System;
using System.ComponentModel.DataAnnotations;
using FloresMvc.Models;

namespace FloresMvc.Models
{
    public class SubCategorias
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdSubcategoria { get; set; }
        public string NomSubcategoria { get; set; }
        public DateTime fecha_Creacion { get; set; }

        public string? Owner { get; set; }          // Quién lo creó
        public DateTime DateOwner { get; set; }     // Cuándo se creó
        public string? LastModify { get; set; }     // Quién lo modificó por última vez
        public DateTime? LastDate { get; set; }
    }
}
