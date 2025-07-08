using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class VisualizacaoBoletosViewModel
    {
        [Display(Name = "Grupo")]
        public int? GrupoIdSelecionado { get; set; }

        [Display(Name = "Paciente")]
        public string? PacienteIdSelecionado { get; set; }

        [Display(Name = "Data da Consulta")]
        [DataType(DataType.Date)]
        public DateTime? DataSelecionada { get; set; }

        public IEnumerable<SelectListItem> Grupos { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Pacientes { get; set; } = new List<SelectListItem>();

        public List<ComprovanteDetalheViewModel> Resultados { get; set; } = new List<ComprovanteDetalheViewModel>();
    }

    public class ComprovanteDetalheViewModel
    {
        public int Id { get; set; }
        public string NomePaciente { get; set; }
        public string UrlComprovante { get; set; }
        public DateTime DataConsulta { get; set; }
    }
}