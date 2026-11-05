using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    /// <summary>
    /// Detalle (insumo) de una remisión.
    /// </summary>
    public class RemisionesDTO
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdDetalle { get; set; }

        [Required]
        [Display(Name = "Remisión")]
        public int IdRemision { get; set; }

        [Required(ErrorMessage = "La referencia es obligatoria.")]
        [StringLength(100)]
        [Display(Name = "Referencia")]
        public string referencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        [Display(Name = "Cantidad")]
        public double Cantidad { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(255)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        // Auditoría
        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        // Relación
        public Remisiones? Remision { get; set; }
    }
}
