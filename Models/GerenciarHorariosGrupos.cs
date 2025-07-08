using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GapsiMVC.ViewModels
{
    public class HorarioGrupoViewModel 
    {
        public int Id { get; set; }
        public string NomeGrupo { get; set; }
        public DayOfWeek DiaDaSemana { get; set; }
        public string DiaDaSemanaFormatado => new CultureInfo("pt-BR").DateTimeFormat.GetDayName(DiaDaSemana);
        public TimeSpan Hora { get; set; }
        public string HoraFormatada => Hora.ToString(@"hh\:mm");
    }

    public class AdicionarHorarioGrupoViewModel
    {
        [Required(ErrorMessage = "Selecione o grupo.")]
        [Display(Name = "Grupo")]
        public int GrupoId { get; set; }

        [Required(ErrorMessage = "Selecione o dia da semana.")]
        [Display(Name = "Dia da Semana")]
        public DayOfWeek DiaDaSemana { get; set; }

        [Required(ErrorMessage = "A hora é obrigatória.")]
        [DataType(DataType.Time)]
        [Display(Name = "Hora (Formato HH:mm)")] 
        public TimeSpan Hora { get; set; }

        public List<SelectListItem> GruposDisponiveis { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DiasDaSemana { get; set; } = new List<SelectListItem>();

        public AdicionarHorarioGrupoViewModel()
        {
            var culturaPtBr = new CultureInfo("pt-BR");
            for (int i = 0; i < 7; i++)
            {
                DiasDaSemana.Add(new SelectListItem
                {
                    Value = ((DayOfWeek)i).ToString(),
                    Text = culturaPtBr.DateTimeFormat.GetDayName((DayOfWeek)i)
                });
            }
        }
    }

    public class AdminGerenciarHorariosViewModel 
    {
        public AdicionarHorarioGrupoViewModel FormularioAdicionar { get; set; } = new AdicionarHorarioGrupoViewModel();
        public List<HorarioGrupoViewModel> HorariosCadastrados { get; set; } = new List<HorarioGrupoViewModel>();
        public int? FiltroGrupoId { get; set; } 
        public List<SelectListItem> GruposParaFiltro { get; set; } = new List<SelectListItem>();
    }
}