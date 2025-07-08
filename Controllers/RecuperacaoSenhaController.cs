using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GapsiMVC.Models;
using GapsiMVC.Services;
using GapsiMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Text.Encodings.Web; 

public class RecuperacaoSenhaController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public RecuperacaoSenhaController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult EsqueciMinhaSenha() 
    {
        return View("~/Views/Login/EsqueciMinhaSenha.cshtml");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EsqueciMinhaSenha(EsqueciMinhaSenhaViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return View("~/Views/Login/ConfirmacaoEnvio.cshtml");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(RedefinirSenha), "RecuperacaoSenha",
                new { email = user.Email, token = token }, protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                model.Email,
                "GapsiGlobal - Redefinição de Senha",
                $"Por favor, redefina sua senha clicando neste link: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Redefinir Senha</a>");

            return View("~/Views/Login/ConfirmacaoEnvio.cshtml");
        }

        return View("~/Views/Login/EsqueciMinhaSenha.cshtml", model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RedefinirSenha(string token = null, string email = null) 
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError(string.Empty, "Um token e e-mail válidos são necessários para redefinir a senha.");
        }
        var model = new RedefinirSenhaViewModel { Token = token, Email = email };

        return View("~/Views/Login/RedefinirSenha.cshtml", model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(RedefinirSenhaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Login/RedefinirSenha.cshtml", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar redefinir a senha. Por favor, tente novamente."); 
            return View("~/Views/Login/RedefinirSenha.cshtml", model); 
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            return View("~/Views/Login/SenhaRedefinida.cshtml");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("~/Views/Login/RedefinirSenha.cshtml", model);
    }
}