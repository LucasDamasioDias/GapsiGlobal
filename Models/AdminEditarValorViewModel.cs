using System.ComponentModel.DataAnnotations;

public class AdminEditarValorViewModel
{
    [Required(ErrorMessage = "O valor é obrigatório.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Novo Valor da Sessão (R$)")]    
    public decimal ValorConsulta { get; set; }
}