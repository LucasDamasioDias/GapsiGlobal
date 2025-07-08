using GapsiMVC.Models;
using GapsiMVC.Services;
using GapsiMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GapsiMVC.Controllers
{
    [Authorize(Roles = "Psicologo")] 
    public class PainelPsicologoController : Controller
    {
        private readonly AtualizarUsuarioService _atualizarUsuarioService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MensagemService _mensagemService;

        public PainelPsicologoController(
            AtualizarUsuarioService atualizarUsuarioService,
            UserManager<ApplicationUser> userManager,
            MensagemService mensagemService)
        {
            _atualizarUsuarioService = atualizarUsuarioService;
            _userManager = userManager;
            _mensagemService = mensagemService;
        }

        [Authorize(Roles = "Psicologo")] 
        public async Task<IActionResult> Index()
        {
            var usuarioLogado = await _userManager.GetUserAsync(User);
            if (usuarioLogado == null)
            {
                return Challenge(); 
            }

            return View("PainelPsicologo", usuarioLogado);
        }

        [HttpGet]
        public async Task<IActionResult> EditarPsicologo()
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
                PhoneNumber = usuario.PhoneNumber
            };
            return View("AtualizarPsicologo", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPsicologo(EditarUsuarioViewModel model)
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
                return View("AtualizarPsicologo", model);
            }

            var (sucesso, erros, mensagensFeedback) = await _atualizarUsuarioService.AtualizarPerfilEGruposAsync(currentUserId, model);
                      
            if (sucesso)
            {
                TempData["MensagemSucesso"] = "Seus dados foram atualizados com sucesso! ";
            }
            else
            {
                foreach (var error in erros)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["MensagemErro"] = "Erro ao atualizar perfil.";

                var usuarioParaRecarregarView = await _atualizarUsuarioService.ObterUsuarioComGruposAsync(currentUserId);
                if (usuarioParaRecarregarView != null)
                {      
                    model.NomeCompleto = string.IsNullOrEmpty(model.NomeCompleto) ? usuarioParaRecarregarView.NomeCompleto : model.NomeCompleto;
                    model.PhoneNumber = string.IsNullOrEmpty(model.PhoneNumber) ? usuarioParaRecarregarView.PhoneNumber : model.PhoneNumber;
                }
                return View("AtualizarPsicologo", model);
            }

            return RedirectToAction("EditarPsicologo");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>AlterarSenhaPsicologo(EditarUsuarioViewModel model) 
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
               
                return View("AtualizarPsicologo", viewModelParaRetorno);
            }

            var userIdentity = await _userManager.FindByIdAsync(currentUserId);
            if (userIdentity == null)
            {
                TempData["MensagemErro"] = "Usuário não encontrado para alteração de senha.";
                return RedirectToAction("EditarPsicologo");
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
                return View("AtualizarPsicologo", viewModelParaRetornoErro);
            }

            TempData["MensagemSucesso"] = "Senha alterada com sucesso!";
            return RedirectToAction("EditarPsicologo");
        }

        [HttpGet]
        public async Task<IActionResult> PsicologoEnviarLinks()
        {
            var viewModel = new EnviarLinkViewModel
            {
                TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync()
            };
            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PsicologoEnviarLinks(EnviarLinkViewModel model)
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
                return RedirectToAction("PsicologoEnviarLinks");
            }
            else
            {
                TempData["MensagemErro"] = $"Erro: {mensagemRetorno}";
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PsicologoEnviarMensagens()
        {
            var viewModel = new EnviarMensagemViewModel
            {
                TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync()
            };
            return View(viewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Psicologo")]
        public async Task<IActionResult> PsicologoEnviarMensagens(EnviarMensagemViewModel model)
        {
            if (model.EnviarParaTodosOsGrupos)
            {
                ModelState.Remove(nameof(model.GruposDestinatariosIds));

                var todosOsGruposSelect = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                model.GruposDestinatariosIds = todosOsGruposSelect
                                                .Select(item => int.Parse(item.Value))
                                                .ToList();
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
                return RedirectToAction(nameof(PsicologoEnviarMensagens));
            }
            else
            {
                TempData["MensagemErro"] = $"Erro ao enviar mensagem: {mensagemRetorno}";
                model.TodosOsGrupos = await _mensagemService.ObterTodosOsGruposParaSelecaoAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarMensagensPsicologo()
        {
            var viewModel = await _mensagemService.ObterMensagensDirecionadasParaPsicologoAsync(User);
            return View(viewModel);
        }
    }
}