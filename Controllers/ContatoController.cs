using GapsiMVC.Services;
using Microsoft.AspNetCore.Mvc;


namespace GapsiMVC.Controllers
{
    public class ContatoController : Controller
    {
        private readonly EmailService _emailService;

        public ContatoController(EmailService emailService)
        {
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View("Contato");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensagem(string nome, string email, string assunto, string mensagem)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(assunto) || string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["MensagemErro"] = "Por favor, preencha todos os campos corretamente.";
                return RedirectToAction("Index");
            }

            try
            {
                await _emailService.EnviarEmailContatoAsync(nome, email, assunto, mensagem);
                TempData["MensagemSucesso"] = "Mensagem enviada com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Response.ContentType = "text/html";

                var mensagemErro = $@"
            <h2>Erro ao enviar e-mail</h2>
            <strong>Mensagem:</strong> {ex.Message}<br/>
            <strong>SMTP:</strong> {_emailService.SmtpServer}<br/>
            <strong>Porta:</strong> {_emailService.Port}<br/>
            <strong>Usuário:</strong> {_emailService.Username}<br/>
        ";

                await Response.WriteAsync(mensagemErro);
                return new EmptyResult(); 
            }
        }
    }
}