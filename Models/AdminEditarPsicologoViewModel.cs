using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GapsiMVC.ViewModels
{
    public class AdminEditarPsicologoViewModel
    {
        public string Id { get; set; } 

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "O CRP é obrigatório.")]
        [Display(Name = "CRP")]
        public string CRP { get; set; }

        [Required(ErrorMessage = "A biografia é obrigatória.")]
        [Display(Name = "Biografia")]
        [DataType(DataType.MultilineText)]
        public string Biografia { get; set; }

        public string FotoUrlAtual { get; set; }

        [Display(Name = "Substituir Foto de Perfil (Opcional)")]
        public IFormFile NovaFoto { get; set; }
    }
}