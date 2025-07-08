using GapsiMVC.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;


namespace GapsiMVC.Services
{
    public class EmailService : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public string SmtpServer => _emailSettings.SmtpServer;
        public int Port => _emailSettings.Port;
        public string Username => _emailSettings.Username;
        public string SenderName => _emailSettings.SenderName;
        public string SenderEmail => _emailSettings.SenderEmail;
        public bool UseSSL => _emailSettings.UseSSL;


        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(toEmail));
            emailMessage.Subject = subject;
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                SecureSocketOptions socketOptions = _emailSettings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, socketOptions);
                if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                }
                await client.SendAsync(emailMessage);
            }
            catch (System.Exception) 
            {
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        public async Task EnviarEmailContatoAsync(string nome, string emailRemetente, string assunto, string mensagem)
        {
            string destinatarioContato = "gapsiglobal@gmail.com";
            string corpoEmail = $"Nome: {nome}\nE-mail do Remetente: {emailRemetente}\nMensagem:\n{mensagem}";
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(destinatarioContato));
            if (!string.IsNullOrEmpty(_emailSettings.SenderEmail))
            {
                emailMessage.Bcc.Add(new MailboxAddress("Backup Contato Gapsi", _emailSettings.SenderEmail));
            }
            emailMessage.Subject = "Contato Site - " + assunto;
            emailMessage.Body = new TextPart("plain") { Text = corpoEmail };

            using var client = new SmtpClient();
            SecureSocketOptions socketOptions = _emailSettings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, socketOptions);
            if (!string.IsNullOrEmpty(_emailSettings.Username) && !string.IsNullOrEmpty(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            }
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}