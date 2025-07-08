using GapsiMVC.Data;
using GapsiMVC.Models;
using GapsiMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GapsiMVC.Services
{
    public class MensagemService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public MensagemService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        
        {
            _context = context;
            _userManager = userManager;
            
        }

        public async Task<List<SelectListItem>> ObterTodosOsGruposParaSelecaoAsync()
        {
            return await _context.Grupos
                .OrderBy(g => g.Nome)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nome })
                .ToListAsync();
        }

        public async Task<(bool Sucesso, string MensagemRetorno)> EnviarMensagemGeralParaGruposAsync(EnviarMensagemViewModel model, ClaimsPrincipal userPrincipal)
        {
            var remetenteUser = await _userManager.GetUserAsync(userPrincipal);
            if (remetenteUser == null || model.GruposDestinatariosIds == null || !model.GruposDestinatariosIds.Any())
            {
                return (false, "Dados inválidos: remetente não encontrado ou nenhum grupo selecionado.");
            }

            var novaMensagem = new MensagemBroadcast
            {
                Conteudo = model.Conteudo,
                DataEnvio = DateTime.Now,
                EnviadoPorUserId = remetenteUser.Id,
            };
            _context.MensagensBroadcast.Add(novaMensagem);
            await _context.SaveChangesAsync();

            int gruposAssociados = 0;
            foreach (var grupoId in model.GruposDestinatariosIds)
            {
                if (await _context.Grupos.AnyAsync(g => g.Id == grupoId))
                {
                    novaMensagem.GruposDestinatarios.Add(new GrupoMensagemBroadcast
                    {
                        MensagemBroadcastId = novaMensagem.Id,
                        GrupoId = grupoId
                    });
                    gruposAssociados++;
                }
            }

            if (gruposAssociados > 0)
            {
                await _context.SaveChangesAsync();
                return (true, $"Mensagem geral registrada para {gruposAssociados} grupo(s).");
            }
            else
            {
                return (false, "Nenhum grupo válido foi associado à mensagem.");
            }
        }


        public async Task<(bool Sucesso, string MensagemRetorno, int DestinatariosAlcancados)> RegistrarLinkReuniaoAsync(EnviarLinkViewModel model, ClaimsPrincipal userPrincipal)
        {
            var remetenteUser = await _userManager.GetUserAsync(userPrincipal);
            if (remetenteUser == null)
            {
                return (false, "Remetente não encontrado.", 0);
            }

            var grupoSelecionado = await _context.Grupos.FindAsync(model.GrupoId);
            if (grupoSelecionado == null)
            {
                return (false, "Grupo selecionado inválido.", 0);
            }

            var horarioFixoDoGrupoParaDia = await ObterHorarioDoGrupoParaDiaAsync(model.GrupoId, model.DataConsulta.DayOfWeek);

            if (horarioFixoDoGrupoParaDia == null)
            {
                return (false, $"Não foi possível encontrar um horário cadastrado para o grupo '{grupoSelecionado.Nome}' no dia da semana selecionado ({model.DataConsulta:dddd}).", 0);
            }
            DateTime dataHoraExataConsulta = model.DataConsulta.Date + horarioFixoDoGrupoParaDia.Value;
            var pacientesComConsultaConfirmada = await _context.Consultas
                .Where(c => c.GrupoId == model.GrupoId &&
                            c.Data == dataHoraExataConsulta &&
                            c.Status == "Confirmada")
                .Select(c => c.Paciente)
                .Where(p => p != null)
                .Distinct()
                .ToListAsync();

            if (!pacientesComConsultaConfirmada.Any())
            {
                return (false, $"Nenhum paciente com consulta confirmada encontrado para o grupo '{grupoSelecionado.Nome}' na data {model.DataConsulta:dd/MM/yyyy} às {horarioFixoDoGrupoParaDia.Value:hh\\:mm}.", 0);
            }

            string conteudoMensagem = $"Segue o link {model.LinkReuniao} de sua consulta do grupo '{grupoSelecionado.Nome}' para o dia {dataHoraExataConsulta:dd/MM/yyyy} às {dataHoraExataConsulta:HH:mm}.";

            var novaMensagem = new MensagemBroadcast
            {
                Conteudo = conteudoMensagem,
                DataEnvio = DateTime.Now,
                EnviadoPorUserId = remetenteUser.Id,
                GrupoDeReferenciaId = grupoSelecionado.Id,
                DataDaConsultaDeReferencia = dataHoraExataConsulta
            };
            _context.MensagensBroadcast.Add(novaMensagem);
            await _context.SaveChangesAsync();

            foreach (var paciente in pacientesComConsultaConfirmada)
            {
                novaMensagem.DestinatariosUsuarios.Add(new UsuarioMensagemBroadcast
                {
                    MensagemBroadcastId = novaMensagem.Id,
                    UsuarioId = paciente.Id
                });
            }

            if (novaMensagem.DestinatariosUsuarios.Any())
            {
                await _context.SaveChangesAsync();
            }

            return (true, $"Link de reunião registrado para {pacientesComConsultaConfirmada.Count} paciente(s).", pacientesComConsultaConfirmada.Count);
        }

        private async Task<TimeSpan?> ObterHorarioDoGrupoParaDiaAsync(int grupoId, DayOfWeek diaDaSemana)
        {
            var horarioGrupo = await _context.HorariosGrupos
                .Where(hg => hg.GrupoId == grupoId && hg.DiaDaSemana == diaDaSemana)
                .Select(hg => hg.Hora) 
                .FirstOrDefaultAsync();

            if (horarioGrupo == default(TimeSpan) && !(await _context.HorariosGrupos.AnyAsync(hg => hg.GrupoId == grupoId && hg.DiaDaSemana == diaDaSemana)))
            {
                Console.WriteLine($"Aviso: Horário não encontrado no banco para GrupoId {grupoId} no dia {diaDaSemana}.");
                return null;
            }

            var horarioDefinido = await _context.HorariosGrupos
               .Where(hg => hg.GrupoId == grupoId && hg.DiaDaSemana == diaDaSemana)
               .FirstOrDefaultAsync();

            if (horarioDefinido != null)
            {
                return horarioDefinido.Hora;
            }

            Console.WriteLine($"Aviso: Horário não encontrado no banco para GrupoId {grupoId} no dia {diaDaSemana}.");
            return null;
        }

        public async Task<PacienteVisualizarMensagensViewModel> ObterMensagensParaPacienteAsync(ClaimsPrincipal userPrincipal)
        {
            var userId = _userManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId))
            {
                return new PacienteVisualizarMensagensViewModel();
            }

            var mensagensDirecionadas = await _context.UsuarioMensagensBroadcast
                .Where(umb => umb.UsuarioId == userId)
                .Include(umb => umb.MensagemBroadcast.EnviadoPorUser)
                .Include(umb => umb.MensagemBroadcast.GrupoDeReferencia)
                .Select(umb => new MensagemParaPacienteViewModel
                {
                    Id = umb.MensagemBroadcast.Id,
                    Conteudo = umb.MensagemBroadcast.Conteudo,
                    DataEnvio = umb.MensagemBroadcast.DataEnvio,
                    EnviadoPorNome = umb.MensagemBroadcast.EnviadoPorUser != null ? umb.MensagemBroadcast.EnviadoPorUser.NomeCompleto : "Sistema",
                    NomeGrupo = umb.MensagemBroadcast.GrupoDeReferencia != null ? umb.MensagemBroadcast.GrupoDeReferencia.Nome : "Link de Reunião"
                })
                .ToListAsync();

            var pacienteComGrupos = await _context.Users
                                        .Include(u => u.Grupos)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.Id == userId);

            var idsGruposDoPaciente = pacienteComGrupos?.Grupos.Select(g => g.Id).ToList() ?? new List<int>();
            var mensagensDeGrupo = new List<MensagemParaPacienteViewModel>();

            if (idsGruposDoPaciente.Any())
            {
                mensagensDeGrupo = await _context.GrupoMensagensBroadcast
                    .Where(gmb => idsGruposDoPaciente.Contains(gmb.GrupoId))
                    .Include(gmb => gmb.MensagemBroadcast.EnviadoPorUser)
                    .Include(gmb => gmb.Grupo)
                    .Select(gmb => new MensagemParaPacienteViewModel
                    {
                        Id = gmb.MensagemBroadcast.Id,
                        Conteudo = gmb.MensagemBroadcast.Conteudo,
                        DataEnvio = gmb.MensagemBroadcast.DataEnvio,
                        EnviadoPorNome = gmb.MensagemBroadcast.EnviadoPorUser != null ? gmb.MensagemBroadcast.EnviadoPorUser.NomeCompleto : "Sistema",
                        NomeGrupo = gmb.Grupo.Nome
                    })
                    .ToListAsync();
            }

            var todasAsMensagens = mensagensDirecionadas.Concat(mensagensDeGrupo)
                                    .GroupBy(m => m.Id)
                                    .Select(g => g.First())
                                    .OrderByDescending(m => m.DataEnvio)
                                    .ToList();

            return new PacienteVisualizarMensagensViewModel { Mensagens = todasAsMensagens };
        }

        public async Task<List<SelectListItem>> ObterTodosOsPsicologosParaDropdownAsync()
        {
            var psicologos = await _userManager.GetUsersInRoleAsync("Psicologo"); 
            return psicologos.Select(p => new SelectListItem
            {
                Value = p.Id,
                Text = $"{p.NomeCompleto} ({p.Email})"
            }).OrderBy(sli => sli.Text).ToList();
        }

        public async Task<(bool Sucesso, string MensagemRetorno)> EnviarMensagemParaPsicologosAsync(AdminEnviarMensagemPsicologosViewModel model, ClaimsPrincipal remetentePrincipal)
        {
            var remetenteUser = await _userManager.GetUserAsync(remetentePrincipal);
            if (remetenteUser == null)
            {
                return (false, "Remetente não autenticado.");
            }

            List<ApplicationUser> psicologosDestinatarios = new List<ApplicationUser>();

            if (model.EnviarParaTodosOsPsicologos)
            {
                psicologosDestinatarios.AddRange(await _userManager.GetUsersInRoleAsync("Psicologo"));
            }
            else
            {
                if (model.DestinatariosPsicologosIds == null || !model.DestinatariosPsicologosIds.Any())
                {
                    return (false, "Nenhum psicólogo destinatário selecionado.");
                }
                foreach (var id in model.DestinatariosPsicologosIds)
                {
                    var psicologo = await _userManager.FindByIdAsync(id);
                    
                    if (psicologo != null && await _userManager.IsInRoleAsync(psicologo, "Psicologo"))
                    {
                        psicologosDestinatarios.Add(psicologo);
                    }
                }
            }

            if (!psicologosDestinatarios.Any())
            {
                return (false, "Nenhum psicólogo válido encontrado para envio.");
            }

            var novaMensagem = new MensagemBroadcast
            {
                Conteudo = model.Conteudo,
                DataEnvio = DateTime.Now,
                EnviadoPorUserId = remetenteUser.Id,
            };

            foreach (var psicologo in psicologosDestinatarios.DistinctBy(p => p.Id)) 
            {
                novaMensagem.DestinatariosUsuarios.Add(new UsuarioMensagemBroadcast
                {
                    UsuarioId = psicologo.Id
                });
            }

            _context.MensagensBroadcast.Add(novaMensagem); 
            await _context.SaveChangesAsync();

            return (true, $"Mensagem enviada para {psicologosDestinatarios.DistinctBy(p => p.Id).Count()} psicólogo(s).");
        }

        public async Task<PsicologoVisualizarMensagensViewModel> ObterMensagensDirecionadasParaPsicologoAsync(ClaimsPrincipal psicologoUserPrincipal)
        {
            var psicologoId = _userManager.GetUserId(psicologoUserPrincipal);
            if (string.IsNullOrEmpty(psicologoId))
            {
                return new PsicologoVisualizarMensagensViewModel(); 
            }

            var mensagens = await _context.UsuarioMensagensBroadcast
                .Where(umb => umb.UsuarioId == psicologoId) 
                .Include(umb => umb.MensagemBroadcast)
                    .ThenInclude(mb => mb.EnviadoPorUser) 
                .OrderByDescending(umb => umb.MensagemBroadcast.DataEnvio)
                .Select(umb => new MensagemParaPsicologoViewModel 
                {
                    Id = umb.MensagemBroadcast.Id,
                    Conteudo = umb.MensagemBroadcast.Conteudo,
                    DataEnvio = umb.MensagemBroadcast.DataEnvio,
                    EnviadoPorNome = umb.MensagemBroadcast.EnviadoPorUser != null ? umb.MensagemBroadcast.EnviadoPorUser.NomeCompleto : "Sistema/Administração"
                })
                .ToListAsync();

            return new PsicologoVisualizarMensagensViewModel { MensagensRecebidas = mensagens };
        }
    }
}
