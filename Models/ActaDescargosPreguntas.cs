using System;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class ActaDescargosPreguntas
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdPregunta { get; set; }

        [Required]
        [Display(Name = "Acta")]
        public int IdActa { get; set; }

        [Required]
        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [Required(ErrorMessage = "La pregunta es obligatoria.")]
        [Display(Name = "Pregunta")]
        public string Pregunta { get; set; } = string.Empty;

        [Required(ErrorMessage = "La respuesta es obligatoria.")]
        [Display(Name = "Respuesta")]
        public string Respuesta { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Owner { get; set; }
        public DateTime? DateOwner { get; set; }

        [MaxLength(30)]
        public string? LastModify { get; set; }
        public DateTime? LastDate { get; set; }

        public ActaDescargos? Acta { get; set; }
    }
}
