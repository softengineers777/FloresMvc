using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class ActaDescargos
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdActa { get; set; }

        [Display(Name = "Ciudad")]
        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [Required]
        [Display(Name = "Fecha de Creación")]
        public DateTime fecha_Creacion { get; set; }

        [Display(Name = "Fecha Informe Disciplinario")]
        [DataType(DataType.Date)]
        public DateTime? FechaInformeDisciplinario { get; set; }

        [Required]
        [Display(Name = "Nombre del Trabajador")]
        [MaxLength(150)]
        public string NombreTrabajador { get; set; } = string.Empty;

        [Required]
        [Display(Name = "C.C. Trabajador")]
        [MaxLength(50)]
        public string CcTrabajador { get; set; } = string.Empty;

        [Display(Name = "Dirección de Residencia")]
        [MaxLength(200)]
        public string? DireccionResidencia { get; set; }

        [Display(Name = "Teléfono")]
        [MaxLength(50)]
        public string? Telefono { get; set; }

        [Required]
        [Display(Name = "Cargo")]
        public int IdCargo { get; set; }

        [Required]
        [Display(Name = "Área")]
        public int IdArea { get; set; }

        [Required]
        [Display(Name = "Proceso")]
        [MaxLength(100)]
        public string Proceso { get; set; } = string.Empty;

        [Display(Name = "Fecha de Ingreso")]
        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        [Display(Name = "Asistido por Testigos")]
        public bool? AsistidoTestigos { get; set; }

        [Display(Name = "Nombre del Testigo")]
        [MaxLength(150)]
        public string? NombreTestigo { get; set; }

        [Display(Name = "Hora Inicio")]
        [MaxLength(20)]
        public string? HoraInicio { get; set; }

        [Display(Name = "Hora Fin")]
        [MaxLength(20)]
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

        [MaxLength(30)]
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }

        [MaxLength(30)]
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        public ICollection<ActaDescargosPreguntas> Preguntas { get; set; } = new List<ActaDescargosPreguntas>();
    }
}
