using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GapsiMVC.Models
{
    public class MensagemBroadcast
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório.")]
        [StringLength(2000, ErrorMessage = "A mensagem não pode exceder 2000 caracteres.")]
        public string Conteudo { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.Now;

        [Required]
        public string EnviadoPorUserId { get; set; }
        public virtual ApplicationUser EnviadoPorUser { get; set; } 

        public int? GrupoDeReferenciaId { get; set; }
        public virtual Grupo GrupoDeReferencia { get; set; } 
        public DateTime? DataDaConsultaDeReferencia { get; set; }

        public virtual ICollection<GrupoMensagemBroadcast> GruposDestinatarios { get; set; } = new List<GrupoMensagemBroadcast>();

        public virtual ICollection<UsuarioMensagemBroadcast> DestinatariosUsuarios { get; set; } = new List<UsuarioMensagemBroadcast>();
    }

    public class GrupoMensagemBroadcast 
    {
        public int MensagemBroadcastId { get; set; }
        public virtual MensagemBroadcast MensagemBroadcast { get; set; } 

        public int GrupoId { get; set; }
        public virtual Grupo Grupo { get; set; } 
    }

    public class UsuarioMensagemBroadcast 
    {
        public int MensagemBroadcastId { get; set; }
        public virtual MensagemBroadcast MensagemBroadcast { get; set; } 

        public string UsuarioId { get; set; }
        public virtual ApplicationUser Usuario { get; set; } 
    }
}