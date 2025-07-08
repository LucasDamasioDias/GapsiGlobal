using GapsiMVC.Models;
using GapsiMVC.ViewModels;
using GapsiMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Data;
using System.Security.Claims;
using System.Globalization;


namespace GapsiMVC.Controllers
{
    [Authorize(Roles = "ADM")] 
    public class PainelAdmController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AgendamentoService _agendamentoService;
        private readonly MensagemService _mensagemService;
        private readonly ApplicationDbContext _context; 
        private readonly AtualizarUsuarioService _atualizarUsuarioService;
        private readonly IS3Service _s3Service;
        public PainelAdmController(
            UserManager<ApplicationUser> userManager,
            AgendamentoService agendamentoService,
            MensagemService mensagemService,
            ApplicationDbContext context,
            AtualizarUsuarioService atualizarUsuarioService,    
            IS3Service s3Service)
        {
            _userManager = userManager;
            _agendamentoService = agendamentoService;
            _mensagemService = mensagemService;
            _context = context;
            _atualizarUsuarioService = atualizarUsuarioService;
            _s3Service = s3Service;
        }



        [Authorize(Roles = "ADM")] 
        public async Task<IActionResult> Index()
        {            
            var usuarioLogado = await _userManager.GetUserAsync(User);

            if (usuarioLogado == null)
            {                
                return Challenge();
            }
            
            return View("PainelAdm", usuarioLogado);
        }

        public async Task<IActionResult> VisualizarPacientes(string searchString)
        {
            var pacientesUsers = await _userManager.GetUsersInRoleAsync("Paciente");
            var pacientesViewModel = new List<PacienteViewModel>();

            foreach (var user in pacientesUsers)
            {
                List<string> gruposNomes = new List<string>();             
                var userWithDetails = await _context.Users
                                                .Include(u => u.Grupos)
                                                .FirstOrDefaultAsync(u => u.Id == user.Id);
                if (userWithDetails != null && userWithDetails.Grupos != null)
                {
                    gruposNomes = userWithDetails.Grupos.Select(g => g.Nome).ToList();
                }

                pacientesViewModel.Add(new PacienteViewModel
                {
                    Id = user.Id,
                    NomeCompleto = user.NomeCompleto,
                    Email = user.Email,
                    Telefone = user.PhoneNumber,
                    Creditos = user.Creditos,
                    Grupos = gruposNomes
                });
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                pacientesViewModel = pacientesViewModel
                    .Where(p => (p.NomeCompleto != null && p.NomeCompleto.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                                (p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
            return View("VisualizarPacientes", pacientesViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GerenciarConsultas(int? grupoId, string status, string nomePaciente)
        {
            var viewModel = await _agendamentoService.ObterConsultasPorGrupoParaAdminAsync(grupoId, status, nomePaciente);
            ViewBag.GruposParaFiltro = new SelectList(viewModel.GruposDisponiveisParaFiltro, "Id", "Nome", grupoId);
            ViewBag.StatusParaFiltro = new SelectList(viewModel.StatusParaFiltro, "Value", "Text", status);
            ViewBag.NomePacientePesquisado = nomePaciente;

            if (grupoId.HasValue && grupoId > 0) { ViewData["TituloFiltro"] = $"Consultas para o Grupo: {viewModel.NomeGrupoSelecionado}"; }
            else { ViewData["TituloFiltro"] = "Todas as Consultas"; }
            if (!string.IsNullOrWhiteSpace(status)) { ViewData["TituloFiltro"] += $" (Status: {status})"; }
            if (!string.IsNullOrWhiteSpace(nomePaciente)) { ViewData["TituloFiltro"] += $" (Paciente: {nomePaciente})"; }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarConsulta(int id, int? grupoId, string status, string nomePaciente)
        {
            bool sucesso = await _agendamentoService.ConfirmarConsultaAdminAsync(id);
            if (sucesso) TempData["MensagemSucesso"] = "Consulta confirmada!"; else TempData["MensagemErro"] = "Erro ao confirmar consulta.";
            return RedirectToAction("GerenciarConsultas", new { grupoId, status, nomePaciente });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConsulta(int id, int? grupoId, string status, string nomePaciente)
        {
            bool sucesso = await _agendamentoService.CancelarConsultaAdminAsync(id);
            if (sucesso) TempData["MensagemSucesso"] = "Consulta cancelada!"; else TempData["MensagemErro"] = "Erro ao cancelar consulta.";
            return RedirectToAction("GerenciarConsultas", new { grupoId, status, nomePaciente });
        }

        [HttpGet]
        [Authorize(Roles = "ADM")]
        public async Task<IActionResult> EditarPaciente(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("ID do paciente não fornecido.");
            var pacienteUser = await _context.Users.Include(u => u.Grupos).FirstOrDefaultAsync(u => u.Id == id);
            if (pacienteUser == null) return NotFound("Paciente não encontrado.");
            if (!await _userManager.IsInRoleAsync(pacienteUser, "Paciente"))
            {
                TempData["MensagemErro"] = "Este usuário não é um paciente.";
                return RedirectToAction("VisualizarPacientes");
            }
            var viewModel = new AdminEditarPacienteViewModel
            {
                Id = pacienteUser.Id,
                NomeCompleto = pacienteUser.NomeCompleto,
                Email = pacienteUser.Email,
                Creditos = pacienteUser.Creditos,
                Grupos = pacienteUser.Grupos?.Select(g => g.Nome).ToList() ?? new List<string>()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADM")]
        public async Task<IActionResult> EditarPaciente(AdminEditarPacienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var pUser = await _context.Users.Include(u => u.Grupos).FirstOrDefaultAsync(u => u.Id == model.Id);
                if (pUser != null) { model.Grupos = pUser.Grupos?.Select(g => g.Nome).ToList() ?? new List<string>(); model.NomeCompleto = pUser.NomeCompleto; model.Email = pUser.Email; }
                return View(model);
            }
            var pacienteUser = await _userManager.FindByIdAsync(model.Id);
            if (pacienteUser == null) return NotFound("Paciente não encontrado.");
            pacienteUser.Creditos = model.Creditos;
            var updateResult = await _userManager.UpdateAsync(pacienteUser);
            if (updateResult.Succeeded)
            {
                TempData["MensagemSucesso"] = "Dados do paciente atualizados!";
                return RedirectToAction("VisualizarPacientes");
            }
            foreach (var error in updateResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
            var pUserRetry = await _context.Users.Include(u => u.Grupos).FirstOrDefaultAsync(u => u.Id == model.Id);
            if (pUserRetry != null) { model.Grupos = pUserRetry.Grupos?.Select(g => g.Nome).ToList() ?? new List<string>(); model.NomeCompleto = pUserRetry.NomeCompleto; model.Email = pUserRetry.Email; }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADM")]
        public async Task<IActionResult> ExcluirPaciente(string id)
        {
            if (string.IsNullOrEmpty(id)) { TempData["MensagemErro"] = "ID do paciente não fornecido."; return RedirectToAction("VisualizarPacientes"); }
            var pacienteUser = await _userManager.FindByIdAsync(id);
            if (pacienteUser == null) { TempData["MensagemErro"] = "Paciente não encontrado."; return RedirectToAction("VisualizarPacientes"); }

            var userWithGroups = await _context.Users.Include(u => u.Grupos).FirstOrDefaultAsync(u => u.Id == id);
            if (userWithGroups?.Grupos != null) { userWithGroups.Grupos.Clear(); await _context.SaveChangesAsync(); }

            var consultasPaciente = await _context.Consultas.Where(c => c.PacienteId == id && c.Data > DateTime.Now && c.Status != "Cancelada" && c.Status != "Realizada").ToListAsync();
            foreach (var c in consultasPaciente) c.Status = "Cancelada (Usuário Excluído)";
            if (consultasPaciente.Any()) await _context.SaveChangesAsync();

            var deleteResult = await _userManager.DeleteAsync(pacienteUser);
            if (deleteResult.Succeeded) TempData["MensagemSucesso"] = "Paciente excluído!"; else TempData["MensagemErro"] = "Erro ao excluir: " + string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return RedirectToAction("VisualizarPacientes");
        }

        [HttpGet]
        public async Task<IActionResult> EnviarMensagens()
        {
            var viewModel = new EnviarMensagemViewModel
            {
                TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync()
            };
            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensagens(EnviarMensagemViewModel model)
        {

            if (model.EnviarParaTodosOsGrupos)
            {                
                ModelState.Remove(nameof(model.GruposDestinatariosIds));                
                model.GruposDestinatariosIds = await _context.Grupos.Select(g => g.Id).ToListAsync();
            }

            if (!ModelState.IsValid)
            {               
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }
           
            var (sucesso, mensagemRetorno) = await _mensagemService.EnviarMensagemGeralParaGruposAsync(model, User);

            if (sucesso)
            {
                TempData["MensagemSucesso"] = mensagemRetorno;
                return RedirectToAction("EnviarMensagens");
            }
            else
            {
                TempData["MensagemErro"] = $"Erro ao enviar mensagem: {mensagemRetorno}";               
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }
        }
                
        [HttpGet]
        public async Task<IActionResult> EnviarLinks()
        {
            var viewModel = new EnviarLinkViewModel
            {
                TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync()
            };
            return View(viewModel);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarLinks(EnviarLinkViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }

            var (sucesso, mensagemRetorno, destinatariosAlcancados) = await _mensagemService.RegistrarLinkReuniaoAsync(model, User);

            if (sucesso)
            {
                TempData["MensagemSucesso"] = mensagemRetorno;
                return RedirectToAction("EnviarLinks");
            }
            else
            {
                TempData["MensagemErro"] = $"Erro: {mensagemRetorno}";
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }
        }
        
        [HttpGet]
        public IActionResult RegistrarPsicologo()
        {
            return View(new AdminRegistrarPsicologoViewModel());                                                                  
        }       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPsicologo(AdminRegistrarPsicologoViewModel model)
        {
            if (ModelState.IsValid)
            {              
                string fotoUrl = null;
                try
                {                   
                    fotoUrl = await _s3Service.UploadFileAsync(model.FotoPerfil, "psicologos");
                    if (string.IsNullOrEmpty(fotoUrl))
                    {
                        throw new Exception("Falha no upload da foto de perfil para o S3.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("FotoPerfil", $"Erro no upload da imagem: {ex.Message}");
                    return View(model); 
                }
               
                var psicologoUser = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    NomeCompleto = model.NomeCompleto,
                    PhoneNumber = model.Telefone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,                   
                    CRP = model.CRP,
                    Biografia = model.Biografia,
                    FotoUrl = fotoUrl
                };

                var result = await _userManager.CreateAsync(psicologoUser, model.Senha);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(psicologoUser, "Psicologo");
                    if (roleResult.Succeeded)
                    {
                        TempData["MensagemSucesso"] = $"Psicólogo '{model.NomeCompleto}' registrado com sucesso!";
                        return RedirectToAction(nameof(RegistrarPsicologo));
                    }
                    else
                    {                        
                        await _userManager.DeleteAsync(psicologoUser);
                        await _s3Service.DeleteFileAsync(fotoUrl);

                        TempData["MensagemErro"] = "Falha ao atribuir a role 'Psicologo'. A operação foi revertida.";
                        foreach (var error in roleResult.Errors) { ModelState.AddModelError(string.Empty, error.Description); }
                    }
                }
                else
                {                    
                    if (!string.IsNullOrEmpty(fotoUrl))
                    {
                        await _s3Service.DeleteFileAsync(fotoUrl);
                    }
                    foreach (var error in result.Errors) { ModelState.AddModelError(string.Empty, error.Description); }
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarPsicologos(string searchString)
        {            
            var psicologosUsers = await _userManager.GetUsersInRoleAsync("Psicologo"); 

            var psicologosViewModel = new List<PsicologoViewModel>();

            foreach (var user in psicologosUsers)
            {                
                var userWithDetails = await _context.Users
                                                .Include(u => u.Grupos)
                                                .FirstOrDefaultAsync(u => u.Id == user.Id);

                List<string> gruposNomes = new List<string>();
                if (userWithDetails != null && userWithDetails.Grupos != null)
                {
                    gruposNomes = userWithDetails.Grupos.Select(g => g.Nome).ToList();
                }

                psicologosViewModel.Add(new PsicologoViewModel
                {
                    Id = user.Id,
                    NomeCompleto = user.NomeCompleto,
                    Email = user.Email,
                    Telefone = user.PhoneNumber,
                    Grupos = gruposNomes                    
                });
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                psicologosViewModel = psicologosViewModel
                    .Where(p => (p.NomeCompleto != null && p.NomeCompleto.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                                (p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return View(psicologosViewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirPsicologo(string id)
        {
            if (string.IsNullOrEmpty(id)) {  }

            var psicologoUser = await _userManager.FindByIdAsync(id);
            if (psicologoUser == null) { }                

            try
            {
                var mensagensAssociadas = _context.UsuarioMensagensBroadcast
                                                .Where(umb => umb.UsuarioId == id);
                _context.UsuarioMensagensBroadcast.RemoveRange(mensagensAssociadas);

                var userWithGroups = await _context.Users
                                            .Include(u => u.Grupos)
                                            .FirstOrDefaultAsync(u => u.Id == id);
                if (userWithGroups?.Grupos != null)
                {
                    userWithGroups.Grupos.Clear();
                }

                if (!string.IsNullOrEmpty(psicologoUser.FotoUrl))
                {
                    await _s3Service.DeleteFileAsync(psicologoUser.FotoUrl);
                }

                await _context.SaveChangesAsync();

                var deleteResult = await _userManager.DeleteAsync(psicologoUser);

                if (deleteResult.Succeeded)
                {
                    TempData["MensagemSucesso"] = "Psicólogo e todos os seus dados associados foram excluídos com sucesso!";
                }
                else
                {
                    throw new Exception("Falha ao deletar o registro principal do usuário após limpar as dependências.");
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemErro"] = "Erro ao excluir psicólogo: " + ex.Message;
            }

            return RedirectToAction(nameof(VisualizarPsicologos));
        }

        [HttpGet]
        public async Task<IActionResult> GerenciarHorariosGrupos(int? filtroGrupoId)
        {
            var viewModel = new AdminGerenciarHorariosViewModel
            {
                HorariosCadastrados = await _agendamentoService.ObterHorariosCadastradosAsync(filtroGrupoId),
                GruposParaFiltro = await _agendamentoService.ObterTodosOsGruposParaDropdownAsync(),
                FiltroGrupoId = filtroGrupoId
            };
            viewModel.FormularioAdicionar.GruposDisponiveis = viewModel.GruposParaFiltro;

            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarHorario(AdminGerenciarHorariosViewModel model) 
        {            
            if (ModelState.IsValid) 
            {
                var (sucesso, mensagem) = await _agendamentoService.AdicionarHorarioGrupoAsync(model.FormularioAdicionar);
                if (sucesso) TempData["MensagemSucesso"] = mensagem; else TempData["MensagemErro"] = mensagem;
            }
            else
            {
                TempData["MensagemErro"] = "Dados inválidos para adicionar horário.";
                model.GruposParaFiltro = await _agendamentoService.ObterTodosOsGruposParaDropdownAsync();
                model.FormularioAdicionar.GruposDisponiveis = model.GruposParaFiltro;
                model.HorariosCadastrados = await _agendamentoService.ObterHorariosCadastradosAsync(model.FiltroGrupoId);
                return View("GerenciarHorariosGrupos", model);
            }
            return RedirectToAction("GerenciarHorariosGrupos", new { filtroGrupoId = model.FormularioAdicionar.GrupoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirHorario(int id, int? filtroGrupoIdAtual)
        {
            var (sucesso, mensagem) = await _agendamentoService.ExcluirHorarioGrupoAsync(id);
            if (sucesso) TempData["MensagemSucesso"] = mensagem; else TempData["MensagemErro"] = mensagem;
            return RedirectToAction("GerenciarHorariosGrupos", new { filtroGrupoId = filtroGrupoIdAtual });
        }

        [HttpGet]
        public async Task<IActionResult> GerenciarGrupos()
        {
            var grupos = await _context.Grupos
                                 .Include(g => g.Usuarios) 
                                 .OrderBy(g => g.Nome)
                                 .ToListAsync();

            var viewModel = new AdminGerenciarGruposViewModel();
            foreach (var grupo in grupos)
            {
                int pacientesNoGrupo = 0;
                foreach (var usuario in grupo.Usuarios)
                {
                    if (await _userManager.IsInRoleAsync(usuario, "Paciente"))
                    {
                        pacientesNoGrupo++;
                    }
                }
                viewModel.GruposExistentes.Add(new GrupoViewModel
                {
                    Id = grupo.Id,
                    Nome = grupo.Nome,
                    NumeroDePacientes = pacientesNoGrupo
                });
            }

            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarGrupo(AdminGerenciarGruposViewModel model)
        {            
            if (ModelState.IsValid)
            {                
                bool nomeExistente = await _context.Grupos.AnyAsync(g => g.Nome.ToLower() == model.NomeNovoGrupo.ToLower());
                if (nomeExistente)
                {
                    TempData["MensagemErro"] = $"Já existe um grupo chamado '{model.NomeNovoGrupo}'.";
                    return RedirectToAction("GerenciarGrupos");
                }

                try
                {                    
                    string imagemUrl = await _s3Service.UploadFileAsync(model.ImagemNovoGrupo, "grupos");

                    if (string.IsNullOrEmpty(imagemUrl))
                    {
                        throw new Exception("Falha no upload da imagem para o S3.");
                    }
                    
                    var novoGrupo = new Grupo
                    {
                        Nome = model.NomeNovoGrupo.Trim(),
                        Descricao = model.DescricaoNovoGrupo.Trim(),
                        ImagemUrl = imagemUrl, 
                        IsDerivado = false,    
                        NomeBase = model.NomeNovoGrupo.Trim()
                    };

                    _context.Grupos.Add(novoGrupo);
                    await _context.SaveChangesAsync();
                    TempData["MensagemSucesso"] = $"Grupo '{novoGrupo.Nome}' criado com sucesso!";
                }
                catch (Exception ex)
                {
                    TempData["MensagemErro"] = $"Ocorreu um erro ao criar o grupo: {ex.Message}";
                }
            }
            else
            {                
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["MensagemErro"] = "Falha na validação. Erros: " + string.Join(" ", errors);
            }

            return RedirectToAction("GerenciarGrupos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirGrupo(int id)
        {
            var grupoParaExcluir = await _context.Grupos
                                            .Include(g => g.Usuarios) 
                                            .Include(g => g.HorariosDefinidos) 
                                            .Include(g => g.Consultas) 
                                            .FirstOrDefaultAsync(g => g.Id == id);

            if (grupoParaExcluir == null)
            {
                TempData["MensagemErro"] = "Grupo não encontrado.";
                return RedirectToAction("GerenciarGrupos");
            }
            
            if (grupoParaExcluir.Usuarios.Any())
            {
                TempData["MensagemErro"] = $"Não é possível excluir o grupo '{grupoParaExcluir.Nome}', pois ele possui usuários associados. Remova os usuários do grupo primeiro.";
                return RedirectToAction("GerenciarGrupos");
            }
            if (grupoParaExcluir.HorariosDefinidos.Any())
            {
                TempData["MensagemErro"] = $"Não é possível excluir o grupo '{grupoParaExcluir.Nome}', pois ele possui horários cadastrados. Remova os horários do grupo primeiro.";
                return RedirectToAction("GerenciarGrupos");
            }
           
            bool temConsultasAtivas = await _context.Consultas.AnyAsync(c => c.GrupoId == id && c.Status != "Cancelada" && c.Status != "Realizada" && c.Data >= DateTime.Today);
            if (temConsultasAtivas)
            {
                TempData["MensagemErro"] = $"Não é possível excluir o grupo '{grupoParaExcluir.Nome}', pois ele possui consultas futuras ou ativas. Cancele ou finalize essas consultas primeiro.";
                return RedirectToAction("GerenciarGrupos");
            }

            try
            {
                _context.Grupos.Remove(grupoParaExcluir);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = $"Grupo '{grupoParaExcluir.Nome}' excluído com sucesso!";
            }
            catch (DbUpdateException ex)
            {
                TempData["MensagemErro"] = $"Erro ao excluir o grupo '{grupoParaExcluir.Nome}'. Verifique se ele ainda possui dependências (consultas, mensagens, etc.). Detalhe: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction("GerenciarGrupos");
        }

        [HttpGet]
        [Authorize(Roles = "ADM")]
        public async Task<IActionResult> EnviarMensagensPsicologos()
        {
            var viewModel = new AdminEnviarMensagemPsicologosViewModel
            {
                TodosOsPsicologosParaSelecao = await _mensagemService.ObterTodosOsPsicologosParaDropdownAsync()
            };
            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADM")]
        public async Task<IActionResult> EnviarMensagensPsicologos(AdminEnviarMensagemPsicologosViewModel model)
        {
            if (!model.EnviarParaTodosOsPsicologos && (model.DestinatariosPsicologosIds == null || !model.DestinatariosPsicologosIds.Any()))
            {
                ModelState.AddModelError(nameof(model.DestinatariosPsicologosIds), "Se não for para todos, selecione ao menos um psicólogo.");
            }

            if (ModelState.IsValid)
            {
                var (sucesso, mensagemRetorno) = await _mensagemService.EnviarMensagemParaPsicologosAsync(model, User);
                if (sucesso)
                {
                    TempData["MensagemSucesso"] = mensagemRetorno;
                    return RedirectToAction("EnviarMensagensPsicologos");
                }
                else
                {
                    TempData["MensagemErro"] = $"Erro: {mensagemRetorno}";
                }
            }

            model.TodosOsPsicologosParaSelecao = await _mensagemService.ObterTodosOsPsicologosParaDropdownAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AlterarDados()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var viewModel = new EditarUsuarioViewModel
            {
                Id = user.Id,
                NomeCompleto = user.NomeCompleto,
                PhoneNumber = user.PhoneNumber,                
                CRP = user.CRP,
                Biografia = user.Biografia,
                FotoUrlAtual = user.FotoUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarDados(EditarUsuarioViewModel model)
        {
            ModelState.Remove(nameof(model.SenhaAtual));
            ModelState.Remove(nameof(model.NovaSenha));
            ModelState.Remove(nameof(model.ConfirmarNovaSenha));  
            if (model.NovaFoto == null || model.NovaFoto.Length == 0)
            {
                ModelState.Remove(nameof(model.NovaFoto));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userToUpdate = await _userManager.GetUserAsync(User);
            if (userToUpdate == null || userToUpdate.Id != model.Id)
            {
                return Forbid();
            }

            try
            {
                userToUpdate.NomeCompleto = model.NomeCompleto;
                userToUpdate.PhoneNumber = model.PhoneNumber;
                userToUpdate.CRP = model.CRP;
                userToUpdate.Biografia = model.Biografia;

                if (model.NovaFoto != null && model.NovaFoto.Length > 0)
                {
                    if (!string.IsNullOrEmpty(userToUpdate.FotoUrl))
                    {
                        await _s3Service.DeleteFileAsync(userToUpdate.FotoUrl);
                    }
                    string novaFotoUrl = await _s3Service.UploadFileAsync(model.NovaFoto, "psicologos");
                    userToUpdate.FotoUrl = novaFotoUrl;
                }

                var result = await _userManager.UpdateAsync(userToUpdate);

                if (result.Succeeded)
                {
                    TempData["MensagemSucesso"] = "Seus dados foram atualizados com sucesso!";
                    return RedirectToAction(nameof(AlterarDados));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocorreu um erro: {ex.Message}");
            }

            return View("AlterarDados", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenhaAdm(EditarUsuarioViewModel model) 
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || (!string.IsNullOrEmpty(model.Id) && model.Id != currentUserId))
            {
                if (string.IsNullOrEmpty(currentUserId)) return Forbid();
            }

            bool houveErroDeValidacaoDeSenha = false;
            if (string.IsNullOrWhiteSpace(model.SenhaAtual))
            {
                ModelState.AddModelError(nameof(model.SenhaAtual), "Senha atual é obrigatória.");
                houveErroDeValidacaoDeSenha = true;
            }
            if (string.IsNullOrWhiteSpace(model.NovaSenha))
            {
                ModelState.AddModelError(nameof(model.NovaSenha), "Nova senha é obrigatória.");
                houveErroDeValidacaoDeSenha = true;
            }
            if (string.IsNullOrWhiteSpace(model.ConfirmarNovaSenha))
            {
                ModelState.AddModelError(nameof(model.ConfirmarNovaSenha), "Confirmação da nova senha é obrigatória.");
                houveErroDeValidacaoDeSenha = true;
            }

            if (!houveErroDeValidacaoDeSenha && model.NovaSenha != model.ConfirmarNovaSenha)
            {
                ModelState.AddModelError(nameof(model.ConfirmarNovaSenha), "A nova senha e a confirmação não coincidem.");
                houveErroDeValidacaoDeSenha = true;
            }

            var errosDeSenhaViewModel = ModelState.Where(kvp =>
                kvp.Key == nameof(model.SenhaAtual) ||
                kvp.Key == nameof(model.NovaSenha) ||
                kvp.Key == nameof(model.ConfirmarNovaSenha) ||
                kvp.Key == "Senha" 
            ).SelectMany(kvp => kvp.Value.Errors).Any();


            if (houveErroDeValidacaoDeSenha || errosDeSenhaViewModel)
            {
                TempData["MensagemErro"] = "Erro ao tentar alterar a senha. Verifique os campos.";                
                var usuarioParaView = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(currentUserId);
                var viewModelParaRetorno = new EditarUsuarioViewModel();
                if (usuarioParaView != null)
                {
                    viewModelParaRetorno.Id = usuarioParaView.Id;
                    viewModelParaRetorno.NomeCompleto = usuarioParaView.NomeCompleto;
                    viewModelParaRetorno.PhoneNumber = usuarioParaView.PhoneNumber;
                }

                return View("AlterarDados", viewModelParaRetorno);
            }

            var userIdentity = await _userManager.FindByIdAsync(currentUserId);
            if (userIdentity == null)
            {
                TempData["MensagemErro"] = "Usuário não encontrado para alteração de senha.";
                return RedirectToAction("AlterarDados");
            }

            var resultado = await _userManager.ChangePasswordAsync(userIdentity, model.SenhaAtual, model.NovaSenha);
            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError("Senha", error.Description);
                }
                TempData["MensagemErro"] = "Falha ao alterar a senha. Verifique os erros.";

                var usuarioParaViewErro = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(currentUserId);
                var viewModelParaRetornoErro = new EditarUsuarioViewModel();
                if (usuarioParaViewErro != null)
                {
                    viewModelParaRetornoErro.Id = usuarioParaViewErro.Id;
                    viewModelParaRetornoErro.NomeCompleto = usuarioParaViewErro.NomeCompleto;
                    viewModelParaRetornoErro.PhoneNumber = usuarioParaViewErro.PhoneNumber;
                }
                return View("AlterarDados", viewModelParaRetornoErro);
            }

            TempData["MensagemSucesso"] = "Senha alterada com sucesso!";
            return RedirectToAction("AlterarDados");
        }

        [HttpGet]
        public async Task<IActionResult> VisualizacaoBoletos()
        {
            var model = new VisualizacaoBoletosViewModel
            {
                Grupos = await _context.Grupos
                    .OrderBy(g => g.Nome)
                    .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nome })
                    .ToListAsync(),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VisualizacaoBoletos(VisualizacaoBoletosViewModel model)
        {
            var query = _context.ComprovantesPagamento.Include(c => c.Usuario).AsQueryable();

            if (model.GrupoIdSelecionado.HasValue)
            {
                query = query.Where(c => c.Usuario.Grupos.Any(g => g.Id == model.GrupoIdSelecionado.Value));
            }

            if (!string.IsNullOrEmpty(model.PacienteIdSelecionado))
            {
                query = query.Where(c => c.IdUsuario == model.PacienteIdSelecionado);
            }

            if (model.DataSelecionada.HasValue)
            {
                var dataFiltro = model.DataSelecionada.Value.Date;
                query = query.Where(c => c.DataConsulta.Date == dataFiltro);
            }

            model.Resultados = await query
                .OrderByDescending(c => c.DataEnvio)
                .Select(c => new ComprovanteDetalheViewModel
                {
                    Id = c.Id,
                    NomePaciente = c.Usuario.NomeCompleto, 
                    UrlComprovante = c.UrlComprovante,
                    DataConsulta = c.DataConsulta
                }).ToListAsync();

            await PopularFiltrosViewModel(model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetPacientesPorGrupo(int grupoId)
        {
            if (grupoId <= 0)
            {
                return Json(new List<object>()); 
            }

            var pacientes = await _context.Users
                .Where(u => u.Grupos.Any(g => g.Id == grupoId))
                .OrderBy(u => u.NomeCompleto)
                .Select(u => new { id = u.Id, text = u.NomeCompleto }) 
                .ToListAsync();

            return Json(pacientes);
        }

        private async Task PopularFiltrosViewModel(VisualizacaoBoletosViewModel model)
        {
            model.Grupos = await _context.Grupos
                .OrderBy(g => g.Nome)
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Nome })
                .ToListAsync();

            if (model.GrupoIdSelecionado.HasValue)
            {
                model.Pacientes = await _context.Users
                    .Where(u => u.Grupos.Any(g => g.Id == model.GrupoIdSelecionado.Value))
                    .OrderBy(u => u.NomeCompleto)
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.NomeCompleto })
                    .ToListAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditarGrupo(int id)
        {
            var grupo = await _context.Grupos.FindAsync(id);
            if (grupo == null)
            {
                return NotFound();
            }

            var viewModel = new AdminEditarGrupoViewModel
            {
                Id = grupo.Id,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                ImagemUrlAtual = grupo.ImagemUrl
            };

            return View(viewModel); 
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarGrupo(AdminEditarGrupoViewModel model)
        {
            if (model.NovaImagem == null || model.NovaImagem.Length == 0)
            {
                ModelState.Remove(nameof(model.NovaImagem));
            }

            if (!ModelState.IsValid)
            {
                var grupoParaErro = await _context.Grupos.FindAsync(model.Id);
                model.ImagemUrlAtual = grupoParaErro?.ImagemUrl;
                return View(model);
            }

            var grupoParaAtualizar = await _context.Grupos.FindAsync(model.Id);
            if (grupoParaAtualizar == null)
            {
                return NotFound();
            }

            try
            {
                grupoParaAtualizar.Nome = model.Nome;
                grupoParaAtualizar.Descricao = model.Descricao;

                if (model.NovaImagem != null && model.NovaImagem.Length > 0)
                {
                    if (!string.IsNullOrEmpty(grupoParaAtualizar.ImagemUrl))
                    {
                        await _s3Service.DeleteFileAsync(grupoParaAtualizar.ImagemUrl);
                    }
                    string novaImagemUrl = await _s3Service.UploadFileAsync(model.NovaImagem, "grupos");
                    grupoParaAtualizar.ImagemUrl = novaImagemUrl;
                }

                _context.Grupos.Update(grupoParaAtualizar);
                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] = "Grupo atualizado com sucesso!";
                return RedirectToAction(nameof(GerenciarGrupos));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocorreu um erro: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditarPsicologo(string id)
        {
            var psicologo = await _userManager.FindByIdAsync(id);
            if (psicologo == null || !await _userManager.IsInRoleAsync(psicologo, "Psicologo"))
            {
                return NotFound();
            }

            var viewModel = new AdminEditarPsicologoViewModel
            {
                Id = psicologo.Id,
                NomeCompleto = psicologo.NomeCompleto,
                CRP = psicologo.CRP,
                Biografia = psicologo.Biografia,
                FotoUrlAtual = psicologo.FotoUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPsicologo(AdminEditarPsicologoViewModel model)
        {
            if (model.NovaFoto == null || model.NovaFoto.Length == 0)
            {
                ModelState.Remove(nameof(model.NovaFoto));
            }

            if (!ModelState.IsValid)
            {
                var psicologoParaErro = await _userManager.FindByIdAsync(model.Id);
                model.FotoUrlAtual = psicologoParaErro?.FotoUrl;
                return View(model);
            }

            var psicologoParaAtualizar = await _userManager.FindByIdAsync(model.Id);
            if (psicologoParaAtualizar == null)
            {
                return NotFound();
            }

            try
            {
                psicologoParaAtualizar.NomeCompleto = model.NomeCompleto;
                psicologoParaAtualizar.CRP = model.CRP;
                psicologoParaAtualizar.Biografia = model.Biografia;

                if (model.NovaFoto != null && model.NovaFoto.Length > 0)
                {
                    if (!string.IsNullOrEmpty(psicologoParaAtualizar.FotoUrl))
                    {
                        await _s3Service.DeleteFileAsync(psicologoParaAtualizar.FotoUrl);
                    }
                    string novaFotoUrl = await _s3Service.UploadFileAsync(model.NovaFoto, "psicologos");
                    psicologoParaAtualizar.FotoUrl = novaFotoUrl;
                }

                var result = await _userManager.UpdateAsync(psicologoParaAtualizar);

                if (result.Succeeded)
                {
                    TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction(nameof(VisualizarPsicologos));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocorreu um erro: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GerenciarValorConsulta()
        {
            var config = await _context.Configuracoes.FindAsync("ValorConsulta");
            var valorAtual = decimal.Parse(config?.Valor ?? "0.00", CultureInfo.InvariantCulture);

            var viewModel = new AdminEditarValorViewModel
            {
                ValorConsulta = valorAtual
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GerenciarValorConsulta(AdminEditarValorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var config = await _context.Configuracoes.FindAsync("ValorConsulta");
                if (config == null)
                {
                    config = new Configuracao { Chave = "ValorConsulta" };
                    _context.Configuracoes.Add(config);
                }

                config.Valor = model.ValorConsulta.ToString("F2", CultureInfo.InvariantCulture);

                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Valor da consulta atualizado com sucesso!";
                return RedirectToAction(nameof(GerenciarValorConsulta));
            }
            return View(model);
        }
    }
}


