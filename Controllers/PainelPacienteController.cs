using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using GapsiMVC.Models; 
using GapsiMVC.Services; 
using GapsiMVC.ViewModels;
using GapsiMVC.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace GapsiMVC.Controllers
{
    [Authorize]
    public class PainelPacienteController : Controller
    {
        private readonly AtualizarUsuarioService _atualizarUsuarioService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MensagemService _mensagemService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IS3Service _s3Service;

        public PainelPacienteController(
            AtualizarUsuarioService atualizarUsuarioService,
            UserManager<ApplicationUser> userManager,
            MensagemService mensagemService,           
            ApplicationDbContext context,
            IEmailSender emailSender,
            IS3Service s3Service
            )
        {
            _atualizarUsuarioService = atualizarUsuarioService;
            _userManager = userManager;
            _mensagemService = mensagemService;            
            _context = context;
            _emailSender = emailSender;
            _s3Service = s3Service;
        }

        [Authorize(Roles = "Paciente")] 
        public async Task<IActionResult> Index()
        {
            var usuarioLogado = await _userManager.GetUserAsync(User);
            if (usuarioLogado == null)
            {
                return Challenge();
            }

            return View("PainelPaciente",usuarioLogado);
        }

        [HttpGet]
        public async Task<IActionResult> Editar()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var usuario = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(userId);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            var viewModel = new EditarUsuarioViewModel
            {
                Id = usuario.Id,
                NomeCompleto = usuario.NomeCompleto,
                PhoneNumber = usuario.PhoneNumber,
                TodosOsGrupos = await _atualizarUsuarioService.ObterTodosOsGruposComSelecaoAsync(usuario)
            };
            return View("atualizarUsuario", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfilEGrupos(EditarUsuarioViewModel model)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || model.Id != currentUserId)
            {
                return Forbid();
            }

            ModelState.Remove(nameof(model.SenhaAtual));
            ModelState.Remove(nameof(model.NovaSenha));
            ModelState.Remove(nameof(model.ConfirmarNovaSenha));

            if (!ModelState.IsValid) 
            {
                var usuarioParaRecarregar = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(currentUserId);
                if (usuarioParaRecarregar != null)
                {
                    model.TodosOsGrupos = await _atualizarUsuarioService.ObterTodosOsGruposComSelecaoAsync(usuarioParaRecarregar);
                }
                else
                {
                    model.TodosOsGrupos = new List<GrupoCheckboxViewModel>();
                }
                return View("atualizarUsuario", model);
            }

            var (sucesso, erros, mensagensFeedback) = await _atualizarUsuarioService.AtualizarPerfilEGruposAsync(currentUserId, model);

            string feedbackCompleto = string.Join(" ", mensagensFeedback ?? new List<string>());

            if (sucesso)
            {
                TempData["MensagemSucesso"] = "Seus dados e grupos foram atualizados com sucesso! " + feedbackCompleto.Trim();
            }
            else
            {
                foreach (var error in erros)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["MensagemErro"] = "Erro ao atualizar perfil e/ou grupos. " + feedbackCompleto.Trim();

                var usuarioParaRecarregarView = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(currentUserId);
                if (usuarioParaRecarregarView != null)
                {
                    model.TodosOsGrupos = await _atualizarUsuarioService.ObterTodosOsGruposComSelecaoAsync(usuarioParaRecarregarView); 
                    model.NomeCompleto = string.IsNullOrEmpty(model.NomeCompleto) ? usuarioParaRecarregarView.NomeCompleto : model.NomeCompleto;
                    model.PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? usuarioParaRecarregarView.PhoneNumber : model.PhoneNumber;
                }
                else
                {
                    model.TodosOsGrupos = new List<GrupoCheckboxViewModel>();
                }
                return View("atualizarUsuario", model);
            }

            return RedirectToAction("Editar");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(EditarUsuarioViewModel model)
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
                    viewModelParaRetorno.TodosOsGrupos = await _atualizarUsuarioService.ObterTodosOsGruposComSelecaoAsync(usuarioParaView);
                }
                else
                {
                    viewModelParaRetorno.Id = model.Id ?? currentUserId;
                    viewModelParaRetorno.TodosOsGrupos = new List<GrupoCheckboxViewModel>();
                }

                return View("atualizarUsuario", viewModelParaRetorno);
            }

            var userIdentity = await _userManager.FindByIdAsync(currentUserId);
            if (userIdentity == null)
            {
                TempData["MensagemErro"] = "Usuário não encontrado para alteração de senha.";
                return RedirectToAction("Editar"); 
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
                    viewModelParaRetornoErro.TodosOsGrupos = await _atualizarUsuarioService.ObterTodosOsGruposComSelecaoAsync(usuarioParaViewErro);
                }
                else
                {
                    viewModelParaRetornoErro.Id = model.Id ?? currentUserId;
                    viewModelParaRetornoErro.TodosOsGrupos = new List<GrupoCheckboxViewModel>();
                }
                return View("atualizarUsuario", viewModelParaRetornoErro);
            }

            TempData["MensagemSucesso"] = "Senha alterada com sucesso!";
            return RedirectToAction("Editar");
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarMensagens()
        {
            var viewModel = await _mensagemService.ObterMensagensParaPacienteAsync(User);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Pagamentos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var grupos = await _context.Grupos
                .Where(g => g.Usuarios.Any(u => u.Id == user.Id))
                .OrderBy(g => g.Nome)
                .ToListAsync();

            var model = new EnvioComprovanteViewModel
            {
                GruposDoPaciente = new SelectList(grupos, "Id", "Nome")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagamentos(EnvioComprovanteViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();
           
            if (!ModelState.IsValid)
            {
                var grupos = await _context.Grupos.Where(g => g.Usuarios.Any(u => u.Id == user.Id)).ToListAsync();
                model.GruposDoPaciente = new SelectList(grupos, "Id", "Nome", model.GrupoId);
                return View(model);
            }

            try
            {                
                string arquivoUrl = await _s3Service.UploadFileAsync(model.ArquivoComprovante, "comprovantes");

                if (string.IsNullOrEmpty(arquivoUrl))
                {
                    throw new Exception("Falha no upload do arquivo para o S3.");
                }
                
                var novoComprovante = new ComprovantePagamento
                {
                    IdUsuario = user.Id,
                    DataConsulta = model.DataConsulta,
                    UrlComprovante = arquivoUrl 
                };
                _context.ComprovantesPagamento.Add(novoComprovante);
                await _context.SaveChangesAsync();
                
                var grupoSelecionado = await _context.Grupos.FindAsync(model.GrupoId);
                var adminEmail = "gapsiglobal@gmail.com";
                var subject = "Novo Comprovante de Pagamento para Análise";
                var message = $@"
            <p>O paciente <strong>{user.NomeCompleto}</strong> (E-mail: {user.Email}) enviou um novo comprovante.</p>
            <h3>Detalhes:</h3>
            <ul>
                <li><strong>Paciente:</strong> {user.NomeCompleto}</li>
                <li><strong>Grupo de Referência:</strong> {grupoSelecionado?.Nome ?? "Não informado"}</li>
                <li><strong>Data da Consulta de Referência:</strong> {model.DataConsulta:dd/MM/yyyy}</li>
            </ul>
            <p>Ação Necessária: Por favor, verifique o pagamento e confirme a consulta do paciente.</p>
            <p style='margin-top: 20px;'>
                <a href='{arquivoUrl}' style='padding: 12px 20px; background-color: #11675e; color: white; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                    Visualizar Comprovante
                </a>
            </p>";

                await _emailSender.SendEmailAsync(adminEmail, subject, message);

                TempData["MensagemSucesso"] = "Comprovante enviado com sucesso! Aguarde a confirmação.";
                return RedirectToAction(nameof(Pagamentos));
            }
            catch (Exception ex)
            {               
                ModelState.AddModelError("", $"Ocorreu um erro ao processar seu comprovante: {ex.Message}");
                var grupos = await _context.Grupos.Where(g => g.Usuarios.Any(u => u.Id == user.Id)).ToListAsync();
                model.GruposDoPaciente = new SelectList(grupos, "Id", "Nome", model.GrupoId);
                return View(model);
            }
        }
    }
}