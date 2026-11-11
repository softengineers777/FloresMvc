using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class DeclaracionConflictoInteres
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdDeclaracion { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        public int IdUsuario { get; set; }

        [MaxLength(10)]
        [Display(Name = "Tipo de documento")]
        public string? TipoDoc { get; set; } = "CC";

        [Required]
        [MaxLength(50)]
        [Display(Name = "Número de documento")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Display(Name = "Cargo")]
        public int? IdCargo { get; set; }

        [Display(Name = "Área")]
        public int? IdArea { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de declaración")]
        public DateTime FechaDeclaracion { get; set; }

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

        [Display(Name = "Firma")]
        public string? Firma { get; set; }

        [MaxLength(30)]
        public string? Owner { get; set; }

        public DateTime DateOwner { get; set; }

        [MaxLength(30)]
        public string? LastModify { get; set; }

        public DateTime? LastDate { get; set; }

        public Usuarios? Usuario { get; set; }
        public Cargo? Cargo { get; set; }
        public Areas? Area { get; set; }

        public ICollection<DeclaracionConflictoInteresPreguntas> Preguntas { get; set; } = new List<DeclaracionConflictoInteresPreguntas>();
    }
}
