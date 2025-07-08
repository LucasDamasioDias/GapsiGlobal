using GapsiMVC.Models;
using GapsiMVC.ViewModels; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;
using System.Text.Encodings.Web;
using GapsiMVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GapsiMVC.Services
{
    public class AgendamentoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;


        public AgendamentoService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<(DayOfWeek Dia, TimeSpan Hora)?> ObterHorarioFixoParaGrupoAsync(int grupoId, DateTime paraData)
        {
            var horarioDefinido = await _context.HorariosGrupos
                .Where(hg => hg.GrupoId == grupoId && hg.DiaDaSemana == paraData.DayOfWeek)
                .Select(hg => new { hg.DiaDaSemana, hg.Hora }) 
                .FirstOrDefaultAsync();

            if (horarioDefinido != null)
            {
                return (horarioDefinido.DiaDaSemana, horarioDefinido.Hora);
            }
            Console.WriteLine($"Aviso: Horário fixo não encontrado no banco para GrupoId {grupoId} no dia {paraData.DayOfWeek}.");
            return null;
        }

        public async Task<AgendarConsultaViewModel> ObterHorariosDisponiveisParaPacienteAsync(ClaimsPrincipal userPrincipal)
        {
            var userId = _userManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId)) return new AgendarConsultaViewModel();

            var paciente = await _context.Users.Include(u => u.Grupos).FirstOrDefaultAsync(u => u.Id == userId);
            if (paciente == null || !paciente.Grupos.Any()) return new AgendarConsultaViewModel();

            var viewModel = new AgendarConsultaViewModel();

            foreach (var grupoDoPaciente in paciente.Grupos)
            {
                var horariosDefinidosParaEsteGrupo = await _context.HorariosGrupos
                    .Where(hg => hg.GrupoId == grupoDoPaciente.Id)
                    .ToListAsync();

                if (!horariosDefinidosParaEsteGrupo.Any()) continue; 

                var horarioVM = new HorarioDisponivelViewModel 
                {
                    GrupoId = grupoDoPaciente.Id,
                    NomeGrupo = grupoDoPaciente.Nome,
                    DiaDaSemana = horariosDefinidosParaEsteGrupo.First().DiaDaSemana,
                    Hora = horariosDefinidosParaEsteGrupo.First().Hora,
                    HorarioFormatado = $"{ObterNomeDiaSemana(horariosDefinidosParaEsteGrupo.First().DiaDaSemana)} às {horariosDefinidosParaEsteGrupo.First().Hora:hh\\:mm}"
                };


                DateTime dataReferencia = DateTime.Today.AddDays(1);
                int slotsParaMostrar = 0; 

                for (int i = 0; i < 28 && slotsParaMostrar < 8; i++) 
                {
                    DateTime dataAtual = dataReferencia.AddDays(i);
                    foreach (var horarioDefinido in horariosDefinidosParaEsteGrupo)
                    {
                        if (dataAtual.DayOfWeek == horarioDefinido.DiaDaSemana)
                        {
                            DateTime dataHoraCompleta = dataAtual.Date + horarioDefinido.Hora;
                            bool jaAgendado = await _context.Consultas
                                .AnyAsync(c => c.GrupoId == grupoDoPaciente.Id &&
                                               c.Data == dataHoraCompleta &&
                                               c.PacienteId == userId &&
                                               c.Status != "Cancelada");
                            if (!jaAgendado)
                            {
                                DateTime dataParaFormulario = new DateTime(dataHoraCompleta.Ticks, DateTimeKind.Unspecified);
                                horarioVM.ProximasDatasDisponiveis.Add(dataParaFormulario);
                                slotsParaMostrar++;
                                if (slotsParaMostrar >= 8) break; 
                            }
                        }
                    }
                    if (slotsParaMostrar >= 8) break;
                }
                if (horarioVM.ProximasDatasDisponiveis.Any())
                {
                    viewModel.HorariosPorGrupo.Add(horarioVM);
                }
            }
            return viewModel;
        }

        public async Task<bool> AgendarConsultaAsync(ClaimsPrincipal userPrincipal, int grupoId, DateTime dataHoraConsulta)
        {
            var userId = _userManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId)) return false;

            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null) return false;
            await _context.Entry(usuario).Collection(u => u.Grupos).LoadAsync();
            if (!usuario.Grupos.Any(g => g.Id == grupoId)) return false;

            var horarioValido = await _context.HorariosGrupos
                .AnyAsync(hg => hg.GrupoId == grupoId &&
                                hg.DiaDaSemana == dataHoraConsulta.DayOfWeek &&
                                hg.Hora == dataHoraConsulta.TimeOfDay);
            if (!horarioValido)
            {
                Console.WriteLine($"Agendamento falhou: Horário {dataHoraConsulta:o} não é válido/definido para GrupoId {grupoId}.");
                return false;
            }

            bool jaAgendado = await _context.Consultas
                .AnyAsync(c => c.GrupoId == grupoId && c.Data == dataHoraConsulta && c.PacienteId == userId && c.Status != "Cancelada");
            if (jaAgendado) return false;

            var novaConsulta = new Consulta { Data = dataHoraConsulta, Status = "Agendada", GrupoId = grupoId, PacienteId = userId };
            _context.Consultas.Add(novaConsulta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SelectListItem>> ObterTodosOsGruposParaDropdownAsync()
        {
            return await _context.Grupos
                .OrderBy(g => g.Nome)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nome })
                .ToListAsync();
        }

        public async Task<List<HorarioGrupoViewModel>> ObterHorariosCadastradosAsync(int? grupoIdFiltro)
        {
            var query = _context.HorariosGrupos.Include(hg => hg.Grupo).AsQueryable();

            if (grupoIdFiltro.HasValue && grupoIdFiltro.Value > 0)
            {
                query = query.Where(hg => hg.GrupoId == grupoIdFiltro.Value);
            }

            return await query
                .OrderBy(hg => hg.Grupo.Nome).ThenBy(hg => hg.DiaDaSemana).ThenBy(hg => hg.Hora)
                .Select(hg => new HorarioGrupoViewModel
                {
                    Id = hg.Id,
                    NomeGrupo = hg.Grupo.Nome,
                    DiaDaSemana = hg.DiaDaSemana,
                    Hora = hg.Hora
                }).ToListAsync();
        }

        public async Task<(bool Sucesso, string Mensagem)> AdicionarHorarioGrupoAsync(AdicionarHorarioGrupoViewModel model)
        {
            if (model.Hora < new TimeSpan(8, 0, 0) || model.Hora > new TimeSpan(20, 0, 0))
            {
                return (false, "A hora deve estar entre 08:00 e 20:00.");
            }

            bool horarioExistente = await _context.HorariosGrupos
                .AnyAsync(hg => hg.GrupoId == model.GrupoId &&
                                hg.DiaDaSemana == model.DiaDaSemana &&
                                hg.Hora == model.Hora);
            if (horarioExistente)
            {
                return (false, "Este horário já está cadastrado para este grupo neste dia da semana.");
            }

            var novoHorario = new HorarioGrupo
            {
                GrupoId = model.GrupoId,
                DiaDaSemana = model.DiaDaSemana,
                Hora = model.Hora
            };

            _context.HorariosGrupos.Add(novoHorario);
            await _context.SaveChangesAsync();
            return (true, "Horário adicionado com sucesso!");
        }

        public async Task<(bool Sucesso, string Mensagem)> ExcluirHorarioGrupoAsync(int horarioId)
        {
            var horario = await _context.HorariosGrupos.FindAsync(horarioId);
            if (horario == null)
            {
                return (false, "Horário não encontrado.");
            }

            _context.HorariosGrupos.Remove(horario);
            await _context.SaveChangesAsync();
            return (true, "Horário excluído com sucesso.");
        }

        private string ObterNomeDiaSemana(DayOfWeek dia)
        {
            return new CultureInfo("pt-BR").DateTimeFormat.GetDayName(dia);
        }

        public async Task<MinhasConsultasViewModel> ObterConsultasDoPacienteAsync(ClaimsPrincipal userPrincipal)
        {
            var userId = _userManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId))
            {
                return new MinhasConsultasViewModel(); 
            }

            var agora = DateTime.Now; 

            var todasConsultasDoPaciente = await _context.Consultas
                .Where(c => c.PacienteId == userId)
                .Include(c => c.Grupo) 
                .OrderByDescending(c => c.Data) 
                .ToListAsync();

            var viewModel = new MinhasConsultasViewModel();

            foreach (var consulta in todasConsultasDoPaciente)
            {
                var consultaVM = new ConsultaViewModel
                {
                    Id = consulta.Id,
                    NomeGrupo = consulta.Grupo?.Nome ?? "Grupo não encontrado", 
                    DataHora = consulta.Data,
                    Status = consulta.Status,
                    PodeCancelarPeloPaciente = consulta.Status == "Agendada" && consulta.Data > agora
                };

                if (consulta.Status == "Cancelada" || consulta.Data < agora)
                {
                    viewModel.ConsultasPassadasOuCanceladas.Add(consultaVM);
                }
                else
                {
                    viewModel.ConsultasAgendadas.Add(consultaVM);
                }
            }
            
            viewModel.ConsultasAgendadas = viewModel.ConsultasAgendadas.OrderBy(c => c.DataHora).ToList();

            return viewModel;
        }

        public async Task<bool> CancelarConsultaAsync(ClaimsPrincipal userPrincipal, int consultaId) 
        {
            var userId = _userManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"LOG: CancelarConsultaAsync - Falha: UserID nulo ou vazio.");
                return false;
            }

            var consulta = await _context.Consultas
                                .Include(c => c.Paciente) 
                                .Include(c => c.Grupo)    
                                .FirstOrDefaultAsync(c => c.Id == consultaId && c.PacienteId == userId);

            if (consulta == null)
            {
                Console.WriteLine($"LOG: CancelarConsultaAsync - Falha: Consulta ID {consultaId} não encontrada para o Paciente ID {userId}.");
                return false; 
            }

            DateTime agoraComTolerancia = DateTime.Now.AddMinutes(-5); 

            if ((consulta.Status != "Agendada" && consulta.Status != "Confirmada") || consulta.Data <= agoraComTolerancia)
            {
                Console.WriteLine($"LOG: CancelarConsultaAsync - Falha: Consulta ID {consultaId} não pode ser cancelada. Status atual: '{consulta.Status}', Data da consulta: {consulta.Data}. (Agora: {agoraComTolerancia})");
                return false;
            }

            consulta.Status = "Cancelada Pelo Paciente"; 

            await _context.SaveChangesAsync();
            Console.WriteLine($"LOG: CancelarConsultaAsync - Sucesso: Consulta ID {consultaId} cancelada pelo Paciente ID {userId}.");

            if (consulta.Paciente != null && !string.IsNullOrEmpty(consulta.Paciente.Email))
            {
                var assunto = "Sua Consulta foi Cancelada - GapsiGlobal";
                var mensagemHtml = $@"
                    <p>Olá {HtmlEncoder.Default.Encode(consulta.Paciente.NomeCompleto ?? "Paciente")},</p>
                    <p>Confirmamos o cancelamento da sua consulta para o grupo '{HtmlEncoder.Default.Encode(consulta.Grupo?.Nome ?? "N/A")}' 
                       que estava agendada para <strong>{consulta.Data:dd/MM/yyyy 'às' HH:mm}</strong>.</p>
                    <p>Este cancelamento foi solicitado por você através do nosso sistema.</p>
                    @* <p>Se aplicável, e de acordo com nossa política, um crédito pode ter sido adicionado à sua conta.</p> *@
                    <p>Se você não solicitou este cancelamento ou tiver alguma dúvida, por favor, entre em contato conosco.</p>
                    <p>Atenciosamente,<br>Equipe GapsiGlobal</p>";
                try
                {
                    await _emailSender.SendEmailAsync(consulta.Paciente.Email, assunto, mensagemHtml);
                    Console.WriteLine($"LOG: CancelarConsultaAsync - E-mail de cancelamento enviado para {consulta.Paciente.Email} para Consulta ID {consultaId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LOG ERROR: CancelarConsultaAsync - Falha ao enviar e-mail de confirmação de cancelamento para {consulta.Paciente.Email} (Consulta ID {consultaId}): {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"LOG WARNING: CancelarConsultaAsync - Não foi possível enviar e-mail de cancelamento para Consulta ID {consultaId}. Paciente ou e-mail do paciente não encontrado na consulta carregada.");
            }
            return true;
        }

        public async Task<List<AdminFiltroGrupoViewModel>> ObterTodosOsGruposParaFiltroAsync()
        {
            return await _context.Grupos
                .OrderBy(g => g.Nome)
                .Select(g => new AdminFiltroGrupoViewModel { Id = g.Id, Nome = g.Nome })
                .ToListAsync();
        }

        public async Task<AdminConsultasViewModel> ObterConsultasPorGrupoParaAdminAsync(int? grupoId, string status, string nomePaciente)
        {
            var viewModel = new AdminConsultasViewModel
            {
                GruposDisponiveisParaFiltro = await ObterTodosOsGruposParaFiltroAsync(),
                GrupoSelecionadoId = grupoId,
                StatusSelecionado = status,
                NomePacientePesquisado = nomePaciente 
            };

            IQueryable<Consulta> query = _context.Consultas.Include(c => c.Paciente).Include(c => c.Grupo);

            if (grupoId.HasValue && grupoId.Value > 0) 
            {
                query = query.Where(c => c.GrupoId == grupoId.Value);
                var grupo = await _context.Grupos.FindAsync(grupoId.Value);
                if (grupo != null)
                {
                    viewModel.NomeGrupoSelecionado = grupo.Nome;
                }
            }
            else 
            {
                viewModel.NomeGrupoSelecionado = "Todos os Grupos";
            }


            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(nomePaciente))
            {
                query = query.Where(c => c.Paciente.NomeCompleto.Contains(nomePaciente) ||
                                         (c.Paciente.Email != null && c.Paciente.Email.Contains(nomePaciente)));
            }

            viewModel.ConsultasDoGrupo = await query
                .OrderBy(c => c.Data)
                .Select(c => new ConsultaViewModel
                {
                    Id = c.Id,
                    NomeGrupo = c.Grupo.Nome,
                    NomePaciente = c.Paciente.NomeCompleto,
                    EmailPaciente = c.Paciente.Email,
                    DataHora = c.Data,
                    Status = c.Status
                })
                .ToListAsync();

            return viewModel;
        }

        public async Task<bool> ConfirmarConsultaAdminAsync(int consultaId)
        {
            var consulta = await _context.Consultas
                                        .Include(c => c.Paciente) 
                                        .Include(c => c.Grupo)    
                                        .FirstOrDefaultAsync(c => c.Id == consultaId);

            if (consulta == null) return false;

            if (consulta.Status == "Agendada")
            {
                consulta.Status = "Confirmada";
                await _context.SaveChangesAsync();

                if (consulta.Paciente != null && !string.IsNullOrEmpty(consulta.Paciente.Email))
                {
                    var assunto = "Sua Consulta foi Confirmada - GapsiGlobal";
                    var mensagemHtml = $@"
                        <p>Olá {HtmlEncoder.Default.Encode(consulta.Paciente.NomeCompleto)},</p>
                        <p>Sua consulta para o grupo '{HtmlEncoder.Default.Encode(consulta.Grupo?.Nome)}' agendada para 
                           <strong>{consulta.Data:dd/MM/yyyy 'às' HH:mm}</strong> foi <strong>CONFIRMADA</strong>.</p>
                        <p>Atenciosamente,<br>Equipe GapsiGlobal</p>";
                    try
                    {
                        await _emailSender.SendEmailAsync(consulta.Paciente.Email, assunto, mensagemHtml);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail de confirmação para {consulta.Paciente.Email}: {ex.Message}");
                    }
                }
                return true;
            }
            return false;
        }

        public async Task<bool> CancelarConsultaAdminAsync(int consultaId)
        {
            var consulta = await _context.Consultas
                                        .Include(c => c.Paciente) 
                                        .Include(c => c.Grupo)    
                                        .FirstOrDefaultAsync(c => c.Id == consultaId);

            if (consulta == null) return false;

            if (consulta.Status != "Cancelada")
            {
                var statusAnterior = consulta.Status; 
                consulta.Status = "Cancelada";
                await _context.SaveChangesAsync();

                if (consulta.Paciente != null && !string.IsNullOrEmpty(consulta.Paciente.Email))
                {
                    var assunto = "Sua Consulta foi Cancelada - GapsiGlobal";
                    var mensagemHtml = $@"
                        <p>Olá {HtmlEncoder.Default.Encode(consulta.Paciente.NomeCompleto)},</p>
                        <p>Informamos que sua consulta para o grupo '{HtmlEncoder.Default.Encode(consulta.Grupo?.Nome)}' que estava agendada para 
                           <strong>{consulta.Data:dd/MM/yyyy 'às' HH:mm}</strong> foi <strong>CANCELADA</strong> pela administração.</p>
                        <p>O valor da consulta será convergido em créditos e abatido do próximo agendamento conforme contrato.</p>
                        <p>Para mais informações ou para reagendar, entre em contato conosco ou acesse o painel do paciente.</p>                        
                        <p>Atenciosamente,<br>Equipe GapsiGlobal</p>";
                    try
                    {
                        await _emailSender.SendEmailAsync(consulta.Paciente.Email, assunto, mensagemHtml);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao enviar e-mail de cancelamento para {consulta.Paciente.Email}: {ex.Message}");
                    }
                }
                return true;
            }
            return false;
        }
    }
}