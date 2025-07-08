using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GapsiMVC.Models;
using GapsiMVC.Data;
using GapsiMVC.ViewModels;



namespace GapsiMVC.Controllers
{
    public class CadastroController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; 
        private readonly AtualizarUsuarioService _atualizarUsuarioService;        

        public CadastroController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, AtualizarUsuarioService atualizarUsuarioService)
        {
            _userManager = userManager;
            _context = context;
            _atualizarUsuarioService = atualizarUsuarioService;
        }

        public async Task<IActionResult> Index() 
        {
            var model = new Cadastro 
            {
                GruposDisponiveis = await _context.Grupos
                                        .Select(g => g.Nome)
                                        .Distinct()
                                        .OrderBy(nome => nome)
                                        .ToListAsync()
            };
            return View("Cadastro", model);
        }


        [HttpPost]
        public IActionResult ValidarCadastro(Cadastro model, List<string> GruposSelecionados) 
        {
            model.GruposSelecionados = GruposSelecionados ?? new List<string>();
            if (model.GruposSelecionados.Count == 0)
            {
                ModelState.AddModelError("GruposSelecionados", "Selecione pelo menos um tema de grupo.");
            }

            if (!ModelState.IsValid)
            {
                model.GruposDisponiveis = _context.Grupos.Select(g => g.Nome).Distinct().OrderBy(n => n).ToList();
                return View("Cadastro", model);
            }
            HttpContext.Session.SetObject("CadastroTemp", model);
            return RedirectToAction("Index", "Contrato");
        }

        [HttpPost]
        public IActionResult ArmazenarTemporario([FromBody] Cadastro model)
        {
            if (model == null)
            {
                return BadRequest(new { sucesso = false, mensagem = "Dados inválidos." });
            }

            try
            {
                HttpContext.Session.SetObject("CadastroTemp", model);
                string idCadastro = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("IdCadastroTemp", idCadastro);
                return Ok(new { sucesso = true, id = idCadastro });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao armazenar temporariamente: {ex.Message}");
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno ao armazenar os dados." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmarCadastro()
        {
            var modelCadastro = HttpContext.Session.GetObject<Cadastro>("CadastroTemp");
            if (modelCadastro == null)
            {
                return BadRequest(new { sucesso = false, mensagem = "Dados do cadastro não encontrados ou sessão expirada." });
            }

            try
            {
                var novoUsuario = new ApplicationUser
                {
                    UserName = modelCadastro.Email,
                    Email = modelCadastro.Email,
                    NomeCompleto = modelCadastro.Nome,
                    PhoneNumber = modelCadastro.Telefone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                };

                var resultCreate = await _userManager.CreateAsync(novoUsuario, modelCadastro.Senha);
                if (!resultCreate.Succeeded)
                {
                    return BadRequest(new { sucesso = false, mensagem = "Erro ao criar usuário.", erros = resultCreate.Errors.Select(e => e.Description) });
                }

                await _userManager.AddToRoleAsync(novoUsuario, "Paciente");

                string feedbackGruposParaUsuario = "Nenhum grupo selecionado ou erro na atribuição.";
                if (modelCadastro.GruposSelecionados != null && modelCadastro.GruposSelecionados.Any())
                {
                    var idsGruposBaseDesejados = new List<int>();
                    foreach (var nomeGrupoBase in modelCadastro.GruposSelecionados.Distinct())
                    {
                        var grupoNoSistema = await _context.Grupos
                            .Where(g => g.Nome == nomeGrupoBase || g.Nome.StartsWith(nomeGrupoBase + " "))
                            .OrderBy(g => g.Nome)
                            .FirstOrDefaultAsync();

                        if (grupoNoSistema != null)
                        {
                            idsGruposBaseDesejados.Add(grupoNoSistema.Id);
                        }
                        else
                        {
                            Console.WriteLine($"AVISO CADASTRO: Grupo base '{nomeGrupoBase}' não encontrado no banco para atribuição inicial.");
                        }
                    }

                    if (idsGruposBaseDesejados.Any())
                    {
                        var pseudoModelParaGrupos = new EditarUsuarioViewModel
                        {
                            Id = novoUsuario.Id,
                            NomeCompleto = novoUsuario.NomeCompleto,
                            PhoneNumber = novoUsuario.PhoneNumber,
                            GruposSelecionadosIds = idsGruposBaseDesejados
                        };

                        var (sucessoGrupos, errosGrupos, mensagensFeedbackServico) = await _atualizarUsuarioService.AtualizarPerfilEGruposAsync(novoUsuario.Id, pseudoModelParaGrupos);

                        if (sucessoGrupos && mensagensFeedbackServico != null && mensagensFeedbackServico.Any())
                        {
                            feedbackGruposParaUsuario = string.Join(" ", mensagensFeedbackServico);
                        }
                        else if (!sucessoGrupos && errosGrupos != null)
                        {
                            feedbackGruposParaUsuario = "Erro ao atribuir grupos: " + string.Join(" ", errosGrupos.Select(e => e.Description));
                        }
                    }
                }

                HttpContext.Session.Remove("CadastroTemp");
                HttpContext.Session.Remove("IdCadastroTemp");

                string mensagemSucessoFinal = "Cadastro realizado com sucesso! " + feedbackGruposParaUsuario;
                TempData["MensagemSucessoGlobal"] = mensagemSucessoFinal; 

                return Ok(new { sucesso = true, mensagem = mensagemSucessoFinal });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado no ConfirmarCadastro: {ex.Message} {ex.StackTrace}");
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno no servidor ao confirmar cadastro." });
            }
        }
    }
}

