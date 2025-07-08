using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GapsiMVC.ViewModels
{
    public class AdminEnviarMensagemPsicologosViewModel
    {
        [Display(Name = "Enviar Para Psicólogo(s)")]
        public List<string> DestinatariosPsicologosIds { get; set; } = new List<string>(); 
        public bool EnviarParaTodosOsPsicologos { get; set; } = false;

        [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório.")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "A mensagem não pode exceder 2000 caracteres.")]
        public string Conteudo { get; set; }
        public List<SelectListItem> TodosOsPsicologosParaSelecao { get; set; } = new List<SelectListItem>();
    }
}