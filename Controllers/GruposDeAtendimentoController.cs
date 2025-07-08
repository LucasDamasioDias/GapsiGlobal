using GapsiMVC.Data;
using GapsiMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using GapsiMVC.Services;


namespace GapsiGlobal.Controllers
{
    public class GruposDeAtendimentoController : Controller
    {
        private readonly ApplicationDbContext _context;        

        public GruposDeAtendimentoController(ApplicationDbContext context) {
                _context = context;                
            }

        public IActionResult Index()
        {
            return View("GruposDeAtendimento");
        }

        [HttpGet]
        public async Task<IActionResult> GruposDeAtendimento() 
        {
            var gruposPrincipais = await _context.Grupos
                .Where(g => g.IsDerivado == false)
                .Include(g => g.HorariosDefinidos)
                .OrderBy(g => g.Nome)
                .ToListAsync();

            var gruposComAgenda = new List<GrupoAtendimentoViewModel>();
            var gruposSemAgenda = new List<GrupoAtendimentoViewModel>();
            var culturaPtBr = new CultureInfo("pt-BR");

            foreach (var grupo in gruposPrincipais)
            {
                var viewModel = new GrupoAtendimentoViewModel
                {
                    Nome = grupo.Nome,
                    Descricao = grupo.Descricao,                 
                    ImagemUrl = grupo.ImagemUrl
                };

                var primeiroHorario = grupo.HorariosDefinidos.FirstOrDefault();

                if (primeiroHorario != null)
                {
                    viewModel.TemAgendaAberta = true;
                    string diaDaSemana = culturaPtBr.TextInfo.ToTitleCase(culturaPtBr.DateTimeFormat.GetDayName(primeiroHorario.DiaDaSemana));
                    string horaFormatada = primeiroHorario.Hora.ToString(@"hh\:mm");
                    viewModel.HorarioFormatado = $"{diaDaSemana} às {horaFormatada}h.";
                    gruposComAgenda.Add(viewModel);
                }
                else
                {
                    viewModel.TemAgendaAberta = false;
                    gruposSemAgenda.Add(viewModel);
                }
            }

            var gruposComAgendaOrdenados = gruposComAgenda.OrderBy(g =>
                gruposPrincipais.First(gp => gp.Nome == g.Nome).HorariosDefinidos.First().DiaDaSemana
            ).ToList();

            var modelParaView = (ComAgenda: gruposComAgendaOrdenados, SemAgenda: gruposSemAgenda);

            return View(modelParaView);
        }
    }
}