using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class DeclaracionConflictoInteresPreguntas
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdDeclaracionPregunta { get; set; }

        [Required]
        [Display(Name = "Declaración")]
        public int IdDeclaracion { get; set; }

        [Required]
        [Display(Name = "Pregunta")]
        public int IdPregunta { get; set; }

        [Display(Name = "Aplica")]
        public bool Aplica { get; set; } = false;

        public DeclaracionConflictoInteres? Declaracion { get; set; }
        public PreguntasConflictoInteres? Pregunta { get; set; }
    }
}
