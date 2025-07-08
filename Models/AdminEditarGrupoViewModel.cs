using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class AdminEditarGrupoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do grupo é obrigatório.")]
        [Display(Name = "Nome do Grupo")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [Display(Name = "Descrição do Grupo")]
        [DataType(DataType.MultilineText)]
        public string Descricao { get; set; }

        public string ImagemUrlAtual { get; set; }

        [Display(Name = "Substituir Imagem (Opcional)")]
        public IFormFile NovaImagem { get; set; }
    }
}