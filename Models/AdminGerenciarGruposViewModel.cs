using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class GrupoViewModel 
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int NumeroDePacientes { get; set; } 
    }

    public class AdminGerenciarGruposViewModel
    {
        public List<GrupoViewModel> GruposExistentes { get; set; } = new List<GrupoViewModel>();

        [Required(ErrorMessage = "O nome do novo grupo é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome do grupo deve ter entre 3 e 100 caracteres.")]
        [Display(Name = "Nome do Novo Grupo")]
        public string NomeNovoGrupo { get; set; } 

        [Required(ErrorMessage = "A descrição do grupo é obrigatória.")]
        [Display(Name = "Descrição do Grupo")]
        public string DescricaoNovoGrupo { get; set; }

        [Required(ErrorMessage = "Uma imagem para o grupo é obrigatória.")]
        [Display(Name = "Imagem do Grupo")]
        public IFormFile ImagemNovoGrupo { get; set; }
    }
}
