using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels 
{
    public class EsqueciMinhaSenhaViewModel
    {
        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Por favor, insira um endereço de e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}