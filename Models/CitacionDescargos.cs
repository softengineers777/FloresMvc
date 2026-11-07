using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class CitacionDescargos
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdCitacion { get; set; }

        [Display(Name = "Ciudad")]
        [MaxLength(50)]
        public string? Ciudad { get; set; }

        [Required]
        [Display(Name = "Fecha de Creación")]
        public DateTime fecha_Creacion { get; set; }

        [Required]
        [Display(Name = "Nombre del Trabajador")]
        [MaxLength(20)]
        public string NomTrabajador { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Cargo")]
        public int IdCargo { get; set; }

        [Required]
        [Display(Name = "Área")]
        public int IdArea { get; set; }

        [Required]
        [Display(Name = "Proceso")]
        [MaxLength(20)]
        public string Proceso { get; set; } = string.Empty;

        [Display(Name = "Fecha de Diligencia")]
        [DataType(DataType.DateTime)]
        public DateTime? fecha_Diligencia { get; set; }

        [Display(Name = "Hora de Diligencia")]
        [DataType(DataType.Time)]
        public DateTime? HoraDiligencia { get; set; }

        [Display(Name = "Fecha de los Hechos")]
        [DataType(DataType.Date)]
        public DateTime? FechaHechos { get; set; }

        [Required]
        [Display(Name = "C.C. Recibido")]
        [MaxLength(20)]
        public string CcRecibido { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }

        [MaxLength(30)]
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }
    }
}
