using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering; 

public class EnvioComprovanteViewModel
{   
    [Required(ErrorMessage = "Por favor, selecione o grupo de referência.")]
    [Display(Name = "Grupo de Referência")]
    public int GrupoId { get; set; }

    [Required(ErrorMessage = "Por favor, selecione a data da consulta.")]
    [Display(Name = "Data da Consulta")]
    [DataType(DataType.Date)]
    public DateTime DataConsulta { get; set; }

    [Required(ErrorMessage = "Por favor, selecione o arquivo do comprovante.")]
    [Display(Name = "Comprovante (Imagem ou PDF)")]
    public IFormFile ArquivoComprovante { get; set; }
    public IEnumerable<SelectListItem> GruposDoPaciente { get; set; } = new List<SelectListItem>();
}