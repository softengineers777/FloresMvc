using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class PreguntasConflictoInteres
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdPregunta { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        public ICollection<DeclaracionConflictoInteresPreguntas> DeclaracionesPreguntas { get; set; } = new List<DeclaracionConflictoInteresPreguntas>();
    }
}
