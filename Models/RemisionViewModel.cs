using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FloresMvc.Models
{
    /// <summary>
    /// Modelo usado por las pantallas de creación y edición de remisiones.
    /// Incluye el encabezado, los insumos y las listas de áreas y fincas.
    /// </summary>
    public class RemisionViewModel
    {
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
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un área.")]
        [Display(Name = "Área")]
        public int IdArea { get; set; }

        [Required(ErrorMessage = "Selecciona una finca.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una finca.")]
        [Display(Name = "Finca")]
        public int IdFinca { get; set; }

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

        // Datos de auditoría (solo lectura en las pantallas).
        public DateTime fecha_Creacion { get; set; }
        public string? Owner { get; set; }
        public DateTime DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        public List<RemisionDetalleViewModel> Detalles { get; set; } = new List<RemisionDetalleViewModel>();

        public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Fincas { get; set; } = new List<SelectListItem>();
    }

    /// <summary>
    /// Fila del detalle capturada en el formulario. Se valida en el
    /// controlador para permitir filas vacías mientras se captura.
    /// </summary>
    public class RemisionDetalleViewModel
    {
        public int IdDetalle { get; set; }

        [Display(Name = "Referencia")]
        public string? Referencia { get; set; }

        [Display(Name = "Cantidad")]
        public double Cantidad { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }
    }
}
