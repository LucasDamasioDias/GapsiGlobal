using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class GrupoCheckboxViewModel
    {
        public int Id { get; set; } 
        public string Nome { get; set; }
        public bool Selecionado { get; set; }
    }

    public class EditarUsuarioViewModel
    {
        public string Id { get; set; } 

        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [Display(Name = "Telefone")]
        public string PhoneNumber { get; set; }
        public List<GrupoCheckboxViewModel> TodosOsGrupos { get; set; } = new List<GrupoCheckboxViewModel>();
        public List<int> GruposSelecionadosIds { get; set; } = new List<int>();

        [Display(Name = "CRP")]
        public string? CRP { get; set; }

        [Display(Name = "Biografia")]
        [DataType(DataType.MultilineText)]
        public string? Biografia { get; set; }

        [Display(Name = "Foto de Perfil Atual")]
        public string? FotoUrlAtual { get; set; }

        [Display(Name = "Substituir Foto (Opcional)")]
        public IFormFile? NovaFoto { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Senha Atual (deixe em branco para não alterar)")]
        public string SenhaAtual { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula, um número e um caractere especial (@$!%*?&).")]
        public string NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a senha de confirmação não coincidem.")]
        public string ConfirmarNovaSenha { get; set; }
    }
}