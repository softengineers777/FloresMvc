using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Inventario
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdInventario { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public required string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo")]
        [Display(Name = "Precio")]
        public double Precio { get; set; }

        public DateTime fecha_Creacion { get; set; }

        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }
    }
}
