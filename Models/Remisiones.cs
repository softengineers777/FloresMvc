using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    /// <summary>
    /// Encabezado de una remisión de ingreso/salida de insumos.
    /// </summary>
    public class Remisiones
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdRemision { get; set; }

        [Required(ErrorMessage = "El número de remisión es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Número de remisión")]
        public string NumeroRemision { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un área.")]
        [Display(Name = "Área")]
        public int IdArea { get; set; }

        [Required(ErrorMessage = "Selecciona una finca.")]
        [Display(Name = "Finca")]
        public int IdFinca { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime fecha_Creacion { get; set; }

        [Required(ErrorMessage = "La placa del vehículo es obligatoria.")]
        [StringLength(20)]
        [Display(Name = "Placa del vehículo")]
        public string PlacaVehiculo { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Despachado por")]
        public string? DespachadoPor { get; set; }

        [StringLength(100)]
        [Display(Name = "Transportado por")]
        public string? TransportadoPor { get; set; }

        [StringLength(100)]
        [Display(Name = "Recibido por")]
        public string? RecibidoPor { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El peso total no puede ser negativo.")]
        [Display(Name = "Peso total")]
        public double PesoTotal { get; set; }

        [StringLength(255)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Auditoría
        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        // Relaciones
        public Areas? Area { get; set; }
        public Fincas? Finca { get; set; }
        public ICollection<RemisionesDTO> Detalles { get; set; } = new List<RemisionesDTO>();
    }
}
