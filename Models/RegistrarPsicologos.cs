using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 

namespace GapsiMVC.ViewModels
{
    public class AdminRegistrarPsicologoViewModel
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; }
        
        [Required(ErrorMessage = "O CRP é obrigatório.")]
        [RegularExpression(@"^\d{2}/\d{4,}$", ErrorMessage = "Formato de CRP inválido. Use o formato XX/XXXX...")]
        [Display(Name = "CRP (Conselho Regional de Psicologia)")]
        public string CRP { get; set; }
        
        [Required(ErrorMessage = "A biografia é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A biografia não pode exceder 2000 caracteres.")]
        [DataType(DataType.MultilineText)] 
        [Display(Name = "Mini Biografia")]
        public string Biografia { get; set; }
        
        [Required(ErrorMessage = "A foto de perfil é obrigatória.")]
        [Display(Name = "Foto de Perfil")]
        public IFormFile FotoPerfil { get; set; }        

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [Display(Name = "E-mail de Acesso")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        [Phone(ErrorMessage = "Formato de telefone inválido.")]
        [Display(Name = "Telefone de Contato")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha Provisória")]
        [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres, uma letra maiúscula, uma letra minúscula, um número e um caractere especial.")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        [Compare("Senha", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        public string ConfirmarSenha { get; set; }
    }
}