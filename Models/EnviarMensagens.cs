using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace GapsiMVC.ViewModels
{
    public class EnviarMensagemViewModel
    {
        [Required(ErrorMessage = "Selecione pelo menos um grupo destinatário.")]
        [Display(Name = "Enviar para Grupo(s)")]
        public List<int> GruposDestinatariosIds { get; set; } = new List<int>();

        [Display(Name = "Enviar para TODOS os Grupos")]
        public bool EnviarParaTodosOsGrupos { get; set; }

        [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório.")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "A mensagem não pode exceder 2000 caracteres.")]
        public string Conteudo { get; set; }

        public List<SelectListItem> TodosOsGrupos { get; set; } = new List<SelectListItem>();
    }
}