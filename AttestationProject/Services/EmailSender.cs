using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AttestationProject.Services                 // ← важный namespace
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config) => _config = config;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_config["Smtp:From"]!));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlBody };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]!),
                true);
            await smtp.AuthenticateAsync(
                _config["Smtp:User"],
                _config["Smtp:Pass"]);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
