using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Configuracao
{
    [Key] 
    [StringLength(50)]
    public string Chave { get; set; }

    [Required]
    public string Valor { get; set; }
}