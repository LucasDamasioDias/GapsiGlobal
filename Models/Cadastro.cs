using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.Models
{
    public class Cadastro
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "A senha deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "A senha deve ter pelo menos 8 caracteres, incluindo letras maíusculas, minúsculas, números e caracteres especiais.")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmarSenha { get; set; }

        [Required(ErrorMessage = "Selecione pelo menos um grupo.")]
        public List<string> GruposSelecionados { get; set; } = new List<string>();
        public List<string> GruposDisponiveis { get; set; } = new List<string>
        {
            "Mulheres Vítimas de Violência",
            "Pais em Luto",
            "Pais Atípicos",
            "Pais Solos",
            "Pessoas com Compulsão Alimentar",
            "Pessoas com Síndrome do Pânico",
            "Pessoas Hipocondríacas",
            "Pessoas Atípicas",
            "Pessoas com Transtornos Alimentares",
            "Profissionais da Saúde e Seus Desafios",
            "Mulheres Vítimas de Violência Sexual",
            "Prevenção ao Suicídio",
            "Pessoas com Dependência em Apostas",
            "Reintegração Social",
            "Famílias de Detentos"
        };
    }
}
