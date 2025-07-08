using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GapsiMVC.Models;

namespace GapsiMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("ADM"))
                {
                    return RedirectToAction("Index", "PainelAdm");
                }else if (User.IsInRole("Psicologo")){
                    return RedirectToAction("Index", "PainelPsicologo");
                }
                else if (User.IsInRole("Paciente"))
                {
                    return RedirectToAction("Index", "PainelPaciente");
                }
                return RedirectToAction("Index", "Home");
            }
            return View("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(string email, string senha) 
        {
            if (ModelState.IsValid) 
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, senha, isPersistent: true, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user); 
                        if (roles.Contains("ADM"))
                        {
                            return RedirectToAction("Index", "PainelAdm");
                        }
                        else if (roles.Contains("Psicologo"))
                        {
                            return RedirectToAction("Index", "PainelPsicologo");
                        }
                        else if (roles.Contains("Paciente"))
                        {
                            return RedirectToAction("Index", "PainelPaciente");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    if (result.IsLockedOut)
                    {
                        ViewBag.Mensagem = "Esta conta está bloqueada. Tente novamente mais tarde.";
                        return View("Login");
                    }
                    if (result.IsNotAllowed)
                    {
                        ViewBag.Mensagem = "Login não permitido para esta conta (verifique se o e-mail foi confirmado, se aplicável).";
                        return View("Login");
                    }
                }
            }

            ViewBag.Mensagem = "E-mail ou senha inválidos!";
            return View("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Login");
        }
    }
}