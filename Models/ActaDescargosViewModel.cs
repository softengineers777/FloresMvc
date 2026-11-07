using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FloresMvc.Models
{
    public class ActaDescargosViewModel
    {
        public int IdActa { get; set; }

        [Display(Name = "Ciudad")]
        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime fecha_Creacion { get; set; }

        [Display(Name = "Fecha Informe Disciplinario")]
        [DataType(DataType.Date)]
        public DateTime? FechaInformeDisciplinario { get; set; }

        [Required(ErrorMessage = "El nombre del trabajador es obligatorio.")]
        [MaxLength(150)]
        [Display(Name = "Nombre del Trabajador")]
        public string NombreTrabajador { get; set; } = string.Empty;

        [Required(ErrorMessage = "La C.C. del trabajador es obligatoria.")]
        [MaxLength(50)]
        [Display(Name = "C.C. Trabajador")]
        public string CcTrabajador { get; set; } = string.Empty;

        [MaxLength(200)]
        [Display(Name = "Dirección de Residencia")]
        public string? DireccionResidencia { get; set; }

        [MaxLength(50)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Selecciona un cargo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cargo.")]
        [Display(Name = "Cargo")]
        public int IdCargo { get; set; }

        [Required(ErrorMessage = "Selecciona un área.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un área.")]
        [Display(Name = "Área")]
        public int IdArea { get; set; }

        [Required(ErrorMessage = "El proceso es obligatorio.")]
        [MaxLength(100)]
        [Display(Name = "Proceso")]
        public string Proceso { get; set; } = string.Empty;

        [Display(Name = "Fecha de Ingreso")]
        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        [Display(Name = "Asistido por Testigos")]
        public bool? AsistidoTestigos { get; set; }

        [MaxLength(150)]
        [Display(Name = "Nombre del Testigo")]
        public string? NombreTestigo { get; set; }

        [MaxLength(20)]
        [Display(Name = "Hora Inicio")]
        public string? HoraInicio { get; set; }

        [MaxLength(20)]
        [Display(Name = "Hora Fin")]
        public string? HoraFin { get; set; }

        [Display(Name = "Fecha de Diligencia")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaDiligencia { get; set; }

        [Display(Name = "Concepto Gestión Humana")]
        public string? ConceptoGestionHumana { get; set; }

        [Display(Name = "Días de Sanción")]
        public int? DiasSancion { get; set; }

        [Display(Name = "Fecha Inicio Sanción")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicioSancion { get; set; }

        // Auditoría (solo lectura)
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        public List<ActaDescargosPreguntaViewModel> Preguntas { get; set; } = new List<ActaDescargosPreguntaViewModel>();

        public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
    }

    public class ActaDescargosPreguntaViewModel
    {
        public int IdPregunta { get; set; }

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Display(Name = "Pregunta")]
        public string? Pregunta { get; set; }

        [Display(Name = "Respuesta")]
        public string? Respuesta { get; set; }
    }
}
