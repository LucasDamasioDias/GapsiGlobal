using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GapsiMVC.ViewModels
{
    public class HorarioDisponivelViewModel
    {
        public int GrupoId { get; set; }
        public string NomeGrupo { get; set; }
        public DayOfWeek DiaDaSemana { get; set; } 
        public TimeSpan Hora { get; set; }         
        public string HorarioFormatado { get; set; } 
        public List<DateTime> ProximasDatasDisponiveis { get; set; } = new List<DateTime>();
    }

    public class AgendarConsultaViewModel
    {
        public List<HorarioDisponivelViewModel> HorariosPorGrupo { get; set; } = new List<HorarioDisponivelViewModel>();
    }

    public class ConfirmarAgendamentoViewModel
    {
        [Required]
        public int GrupoId { get; set; }
        public string NomeGrupo { get; set; }

        [Required(ErrorMessage = "A data e hora da consulta são obrigatórias.")]
        [DataType(DataType.DateTime)]
        public DateTime DataHoraConsulta { get; set; }
        public string DataHoraFormatada => DataHoraConsulta.ToString("dd/MM/yyyy 'às' HH:mm");
    }
    public class ConsultaViewModel
    {
        public int Id { get; set; }
        public string NomeGrupo { get; set; }
        public string NomePaciente { get; set; }
        public string EmailPaciente { get; set; } 
        public DateTime DataHora { get; set; }
        public string DataHoraFormatada => DataHora.ToString("dd/MM/yyyy 'às' HH:mm");
        public string Status { get; set; }
        public bool PodeCancelarPeloPaciente { get; set; }  
    }

    public class MinhasConsultasViewModel
    {
        public List<ConsultaViewModel> ConsultasAgendadas { get; set; } = new List<ConsultaViewModel>();
        public List<ConsultaViewModel> ConsultasPassadasOuCanceladas { get; set; } = new List<ConsultaViewModel>();
    }

    public class AdminFiltroGrupoViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public class AdminConsultasViewModel
    {
        public List<AdminFiltroGrupoViewModel> GruposDisponiveisParaFiltro { get; set; } = new List<AdminFiltroGrupoViewModel>();
        public int? GrupoSelecionadoId { get; set; }
        public string NomeGrupoSelecionado { get; set; }
        public string StatusSelecionado { get; set; }
        public string NomePacientePesquisado { get; set; }

        public List<ConsultaViewModel> ConsultasDoGrupo { get; set; } = new List<ConsultaViewModel>();
        public List<SelectListItem> StatusParaFiltro { get; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "", Text = "Todos os Status" },
        new SelectListItem { Value = "Agendada", Text = "Agendada" },
        new SelectListItem { Value = "Confirmada", Text = "Confirmada" },
        new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
    };
  }
}