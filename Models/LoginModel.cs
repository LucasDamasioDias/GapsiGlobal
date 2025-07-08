using System.ComponentModel.DataAnnotations;

namespace GapsiGlobal.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
}
