using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.Models
{
    public class HorarioGrupo
    {
        public int Id { get; set; }

        [Required]
        public int GrupoId { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required(ErrorMessage = "O dia da semana é obrigatório.")]
        public DayOfWeek DiaDaSemana { get; set; } 

        [Required(ErrorMessage = "A hora é obrigatória.")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = @"{0:hh\:mm}", ApplyFormatInEditMode = true)] 
        public TimeSpan Hora { get; set; } 
    }
}