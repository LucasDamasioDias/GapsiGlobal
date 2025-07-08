using GapsiMVC.Services;
using GapsiMVC.ViewModels; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GapsiMVC.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class AgendamentoController : Controller
    {
        private readonly AgendamentoService _agendamentoService;

        public AgendamentoController(AgendamentoService agendamentoService)
        {
            _agendamentoService = agendamentoService;
        }
       
        public async Task<IActionResult> Index()
        {
            var viewModel = await _agendamentoService.ObterHorariosDisponiveisParaPacienteAsync(User);

            if (!viewModel.HorariosPorGrupo.Any() && TempData["MensagemInfo"] == null)
            {
                TempData["MensagemInfo"] = "Você não está cadastrado em grupos com horários de agendamento definidos ou não há horários disponíveis no momento.";
            }

            return View("~/Views/PainelPaciente/Agendamento.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(ConfirmarAgendamentoViewModel model) 
        {
            if (ModelState.IsValid)
            {
                bool sucesso = await _agendamentoService.AgendarConsultaAsync(User, model.GrupoId, model.DataHoraConsulta);

                if (sucesso)
                {
                    TempData["MensagemSucesso"] = $"Consulta para o grupo '{model.NomeGrupo}' agendada com sucesso para {model.DataHoraFormatada}!";
                    return RedirectToAction("MinhasConsultas");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Não foi possível realizar o agendamento. Verifique se o horário ainda está disponível ou se você já possui uma consulta neste horário.");
                }
            }

            var horariosViewModel = await _agendamentoService.ObterHorariosDisponiveisParaPacienteAsync(User);      
            return View("~/Views/PainelPaciente/Agendamento.cshtml", horariosViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MinhasConsultas()
        {
            var viewModel = await _agendamentoService.ObterConsultasDoPacienteAsync(User);
            return View("~/Views/PainelPaciente/MinhasConsultas.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConsulta(int id) 
        {
            bool sucesso = await _agendamentoService.CancelarConsultaAsync(User, id);

            if (sucesso)
            {
                TempData["MensagemSucesso"] = "Consulta cancelada com sucesso!";
            }
            else
            {
                TempData["MensagemErro"] = "Não foi possível cancelar a consulta. Verifique se ela ainda pode ser cancelada ou se você tem permissão.";
            }
            return RedirectToAction("MinhasConsultas");
        }
    }
}