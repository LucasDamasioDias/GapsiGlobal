using GapsiMVC.Models;
using GapsiMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GapsiMVC.Controllers
{
    public class QuemSomosController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;       
        private const string ID_BRUNA = "E6C6BA98-A925-4AB5-B261-3F6D9B0CDFD6";
        private const string ID_GLORIA = "5D79944D-017F-4591-B1F4-526DAA9B6F69";

        public QuemSomosController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new QuemSomosViewModel();

            var brunaUser = await _userManager.FindByIdAsync(ID_BRUNA);
            if (brunaUser != null)
            {
                viewModel.SociaBruna = new PerfilProfissionalViewModel
                {
                    NomeCompleto = brunaUser.NomeCompleto,
                    CRP = brunaUser.CRP,
                    Biografia = brunaUser.Biografia,
                    FotoUrl = brunaUser.FotoUrl
                };
            }

            var gloriaUser = await _userManager.FindByIdAsync(ID_GLORIA);
            if (gloriaUser != null)
            {
                viewModel.SociaGloria = new PerfilProfissionalViewModel
                {
                    NomeCompleto = gloriaUser.NomeCompleto,
                    CRP = gloriaUser.CRP,
                    Biografia = gloriaUser.Biografia,
                    FotoUrl = gloriaUser.FotoUrl
                };
            }

            var todosOsPsicologos = await _userManager.GetUsersInRoleAsync("Psicologo");
            if (todosOsPsicologos != null)
            {
                var parceiros = todosOsPsicologos
                    .Where(u => u.Id != ID_BRUNA && u.Id != ID_GLORIA && !string.IsNullOrEmpty(u.CRP))
                    .OrderBy(u => u.NomeCompleto);

                foreach (var user in parceiros)
                {
                    viewModel.PsicologosParceiros.Add(new PerfilProfissionalViewModel
                    {
                        NomeCompleto = user.NomeCompleto,
                        CRP = user.CRP,
                        Biografia = user.Biografia,
                        FotoUrl = user.FotoUrl
                    });
                }
            }

            return View("quemSomos", viewModel);
        }
    }
}