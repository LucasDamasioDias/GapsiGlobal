using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class AdminEditarPacienteViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } 

        [Display(Name = "E-mail")]
        public string Email { get; set; } 

        [Required(ErrorMessage = "O campo Créditos é obrigatório.")]
        [Range(0, int.MaxValue, ErrorMessage = "Créditos não podem ser negativos.")]
        [Display(Name = "Créditos")]
        public int Creditos { get; set; }
        public List<string> Grupos { get; set; } = new List<string>();
    }
}