using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FloresMvc.Models
{
    public class DeclaracionConflictoInteresViewModel
    {
        public int IdDeclaracion { get; set; }

        [Required(ErrorMessage = "Seleccione un usuario.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un usuario.")]
        [Display(Name = "Usuario")]
        public int IdUsuario { get; set; }

        [MaxLength(10)]
        [Display(Name = "Tipo de documento")]
        public string? TipoDoc { get; set; } = "CC";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [MaxLength(50)]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Display(Name = "Cargo")]
        public int? IdCargo { get; set; }

        [Display(Name = "Área")]
        public int? IdArea { get; set; }

        [Required(ErrorMessage = "La fecha de declaración es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de declaración")]
        public DateTime FechaDeclaracion { get; set; } = DateTime.Today;

        [MaxLength(150)]
        [Display(Name = "Nombre del familiar")]
        public string? NombreFamiliar { get; set; }

        [MaxLength(150)]
        [Display(Name = "Cargo del familiar")]
        public string? CargoFamiliar { get; set; }

        [MaxLength(150)]
        [Display(Name = "Área del familiar")]
        public string? AreaFamiliar { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "La firma es obligatoria.")]
        [Display(Name = "Firma")]
        public string? Firma { get; set; }

        // Auditoría (solo lectura)
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        public List<DeclaracionConflictoPreguntaItemViewModel> Preguntas { get; set; } = new();

        public IEnumerable<SelectListItem> Usuarios { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Cargos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Areas { get; set; } = new List<SelectListItem>();
    }

    public class DeclaracionConflictoPreguntaItemViewModel
    {
        public int IdDeclaracionPregunta { get; set; }
        public int IdPregunta { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Aplica { get; set; }
    }
}
