using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class RedefinirSenhaViewModel 
    {
        [Required(ErrorMessage = "O campo E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo Nova Senha é obrigatório.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        [StringLength(100, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula, um número e um caractere especial (@$!%*?&).")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        [Compare("Password", ErrorMessage = "A nova senha e a senha de confirmação não coincidem.")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}