using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.Models
{
        public class ComprovantePagamento
        {
            public int Id { get; set; }

            [Required]
            public string IdUsuario { get; set; } 

            [ForeignKey("IdUsuario")]
            public virtual ApplicationUser Usuario { get; set; }

            public DateTime DataConsulta { get; set; }

            public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

            [Required]
            public string UrlComprovante { get; set; } 
        }
    }

