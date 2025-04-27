using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using System.Net;

namespace GateHub.repository
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];

           var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);
            
         await smtpClient.SendMailAsync(mailMessage);


        }
    }
}
