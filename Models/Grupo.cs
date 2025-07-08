using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.Models
{
    public class Grupo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(1000)]
        public string Descricao { get; set; }

        [StringLength(500)]
        public string ImagemUrl { get; set; }

        public bool IsDerivado { get; set; } = false;

        [StringLength(100)]
        public string NomeBase { get; set; }

        public virtual ICollection<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<HorarioGrupo> HorariosDefinidos { get; set; } = new List<HorarioGrupo>();
        public virtual ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
    }
}