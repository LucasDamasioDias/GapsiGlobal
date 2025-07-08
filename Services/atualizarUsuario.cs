using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;
using GapsiMVC.ViewModels;
using GapsiMVC.Data;
using GapsiMVC.Services;
using System.Text.RegularExpressions; 

public class AtualizarUsuarioService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender; 
    private const int LIMITE_PACIENTES_POR_GRUPO = 8; 

    public AtualizarUsuarioService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender; 
    }

    public async Task<ApplicationUser> ObterUsuarioComGruposAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        return await _userManager.Users
            .Include(u => u.Grupos)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<List<GrupoCheckboxViewModel>> ObterTodosOsGruposComSelecaoAsync(ApplicationUser usuario)
    {
        var todosOsGruposDoSistema = await _context.Grupos.OrderBy(g => g.Nome).ToListAsync();
        var gruposViewModel = new List<GrupoCheckboxViewModel>();
        var idsGruposDoUsuario = usuario?.Grupos?.Select(g => g.Id).ToList() ?? new List<int>();

        foreach (var grupoSistema in todosOsGruposDoSistema)
        {
            int pacientesNoGrupo = await ContarPacientesNoGrupoAsync(grupoSistema.Id);
            bool estaCheio = pacientesNoGrupo >= LIMITE_PACIENTES_POR_GRUPO;
            bool usuarioEstaNoGrupo = idsGruposDoUsuario.Contains(grupoSistema.Id);

            gruposViewModel.Add(new GrupoCheckboxViewModel
            {
                Id = grupoSistema.Id,
                Nome = grupoSistema.Nome + (estaCheio && !usuarioEstaNoGrupo ? " (Lotado)" : ""), 
                Selecionado = usuarioEstaNoGrupo,                
            });
        }
        return gruposViewModel;
    }


    private async Task<int> ContarPacientesNoGrupoAsync(int grupoId)
    {
        var grupo = await _context.Grupos.Include(g => g.Usuarios).FirstOrDefaultAsync(g => g.Id == grupoId);
        if (grupo == null) return 0;
        int count = 0;
        foreach (var usuario in grupo.Usuarios)
        {
            if (await _userManager.IsInRoleAsync(usuario, "Paciente")) count++;
        }
        return count;
    }

    private string ExtrairNomeBaseGrupo(string nomeCompletoGrupo)
    {
        var match = Regex.Match(nomeCompletoGrupo, @"^(.*?)( \d+)?$");
        return match.Groups[1].Value.Trim();
    }

    private async Task<Grupo> EncontrarOuCriarGrupoDisponivelAsync(string nomeBaseGrupo, string nomeGrupoOriginalSelecionado)
    {
        var gruposComMesmoNomeBase = await _context.Grupos
            .Where(g => g.NomeBase == nomeBaseGrupo)
            .Include(g => g.Usuarios) 
            .OrderBy(g => g.Nome) 
            .ToListAsync();

        Grupo grupoDisponivel = null;

        var grupoOriginal = gruposComMesmoNomeBase.FirstOrDefault(g => g.Nome == nomeGrupoOriginalSelecionado);
        if (grupoOriginal != null && await ContarPacientesNoGrupoAsync(grupoOriginal.Id) < LIMITE_PACIENTES_POR_GRUPO)
        {
            grupoDisponivel = grupoOriginal;
        }
        else
        {
            foreach (var grupo in gruposComMesmoNomeBase)
            {
                if (await ContarPacientesNoGrupoAsync(grupo.Id) < LIMITE_PACIENTES_POR_GRUPO)
                {
                    grupoDisponivel = grupo;
                    break; 
                }
            }
        }

        if (grupoDisponivel == null)
        {
            var grupoPrincipal = gruposComMesmoNomeBase.FirstOrDefault(g => !g.IsDerivado);

            if (grupoPrincipal == null) grupoPrincipal = gruposComMesmoNomeBase.FirstOrDefault();

            if (grupoPrincipal == null)
            {
                return null;
            }

            int sufixo = 2;
            string nomeNovoGrupo;
            var ultimoGrupo = gruposComMesmoNomeBase.LastOrDefault(g => Regex.IsMatch(g.Nome, @" \d+$"));
            if (ultimoGrupo != null)
            {
                sufixo = int.Parse(Regex.Match(ultimoGrupo.Nome, @"(\d+)$").Value) + 1;
            }

            do
            {
                nomeNovoGrupo = $"{nomeBaseGrupo} {sufixo}";
                sufixo++;
            } while (await _context.Grupos.AnyAsync(g => g.Nome == nomeNovoGrupo));

            grupoDisponivel = new Grupo
            {
                Nome = nomeNovoGrupo,
                NomeBase = grupoPrincipal.NomeBase,      
                Descricao = grupoPrincipal.Descricao,   
                ImagemUrl = grupoPrincipal.ImagemUrl,   
                IsDerivado = true                      
            };

            _context.Grupos.Add(grupoDisponivel); 

            try
            {
                await _emailSender.SendEmailAsync(
                    "gapsiglobal@gmail.com",
                    "Novo Grupo Criado Automaticamente - GapsiGlobal",
                    $"<p>O(s) grupo(s) com nome base '<strong>{nomeBaseGrupo}</strong>' atingiram o limite de pacientes.</p>" +
                    $"<p>Um novo grupo chamado '<strong>{grupoDisponivel.Nome}</strong>' foi criado automaticamente.</p>" +
                    "<p>Ele foi criado com a mesma descrição e imagem do grupo principal. Verifique se os horários precisam ser configurados no painel administrativo.</p>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao notificar ADM sobre novo grupo: {ex.Message}");
            }
        }

        return grupoDisponivel;
    }

    public async Task<(bool Sucesso, IEnumerable<IdentityError> Erros, List<string> MensagensFeedback)> AtualizarPerfilEGruposAsync(string userId, EditarUsuarioViewModel model)
    {
        var usuarioExistente = await _userManager.Users
                                            .Include(u => u.Grupos)
                                            .FirstOrDefaultAsync(u => u.Id == userId);

        if (usuarioExistente == null)
        {
            return (false, new List<IdentityError> { new IdentityError { Description = "Usuário não encontrado." } }, null);
        }

        usuarioExistente.NomeCompleto = model.NomeCompleto;
        usuarioExistente.PhoneNumber = model.PhoneNumber;

        var mensagensFeedback = new List<string>();

        var idsGruposAtuaisDoUsuario = usuarioExistente.Grupos.Select(g => g.Id).ToList();
        var gruposParaRemoverDoUsuario = usuarioExistente.Grupos
            .Where(g => !model.GruposSelecionadosIds.Contains(g.Id))
            .ToList();
        foreach (var grupo in gruposParaRemoverDoUsuario)
        {
            usuarioExistente.Grupos.Remove(grupo);
            mensagensFeedback.Add($"Removido do grupo '{grupo.Nome}'.");
        }

        var idsGruposQueUsuarioQuerEntrar = model.GruposSelecionadosIds
            .Where(idSelecionado => !idsGruposAtuaisDoUsuario.Contains(idSelecionado) || 
                                    gruposParaRemoverDoUsuario.Any(gr => gr.Id == idSelecionado)) 
            .Distinct()
            .ToList();

        foreach (var grupoIdDesejado in idsGruposQueUsuarioQuerEntrar)
        {
            var grupoOriginalSelecionado = await _context.Grupos.FindAsync(grupoIdDesejado);
            if (grupoOriginalSelecionado == null)
            {
                mensagensFeedback.Add($"Tentativa de adicionar ao grupo ID {grupoIdDesejado}, mas o grupo não foi encontrado.");
                continue;
            }

            string nomeBaseOriginal = ExtrairNomeBaseGrupo(grupoOriginalSelecionado.Nome);
            Grupo grupoParaAlocar = await EncontrarOuCriarGrupoDisponivelAsync(nomeBaseOriginal, grupoOriginalSelecionado.Nome);

            if (grupoParaAlocar != null)
            {
                if (!usuarioExistente.Grupos.Any(g => g.Id == grupoParaAlocar.Id))
                {
                    if (_context.Entry(grupoParaAlocar).State == EntityState.Detached)
                    {
                        _context.Grupos.Attach(grupoParaAlocar);
                    }
                    usuarioExistente.Grupos.Add(grupoParaAlocar);
                    mensagensFeedback.Add($"Adicionado ao grupo '{grupoParaAlocar.Nome}'.");
                }
            }
            else
            {
                mensagensFeedback.Add($"Não foi possível alocar você no tema do grupo '{grupoOriginalSelecionado.Nome}'. Todos os grupos relacionados estão lotados e não foi possível criar um novo.");
            }
        }

        var resultadoUpdate = await _userManager.UpdateAsync(usuarioExistente); 

        if (!resultadoUpdate.Succeeded)
        {
            Console.WriteLine("❌ Erro ao atualizar perfil e/ou grupos do usuário: " + string.Join(", ", resultadoUpdate.Errors.Select(e => e.Description)));
        }
        return (resultadoUpdate.Succeeded, resultadoUpdate.Errors, mensagensFeedback);
    }

    public async Task<bool> AtualizarPacienteAsync(string userId, string nome, string telefone)
    {
        var usuarioExistente = await _userManager.FindByIdAsync(userId);
        if (usuarioExistente == null) return false;

        usuarioExistente.NomeCompleto = nome;
        usuarioExistente.PhoneNumber = telefone;
        var resultado = await _userManager.UpdateAsync(usuarioExistente);
        return resultado.Succeeded;
    }
}