using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AttestationProject.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            // Если не настроен Smtp:Host — просто логируем и выходим
            var host = _config["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("SMTP disabled, skipping sending email to {Email}", to);
                return;
            }

            // Читаем остальные параметры
            var portValue = _config["Smtp:Port"];
            if (!int.TryParse(portValue, out var port))
            {
                _logger.LogError("Invalid Smtp:Port value '{Port}'", portValue);
                return;
            }

            var from = _config["Smtp:From"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];
            if (string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(pass))
            {
                _logger.LogError("SMTP credentials incomplete (From/User/Pass)");
                return;
            }

            try
            {
                var msg = new MimeMessage();
                msg.From.Add(MailboxAddress.Parse(from));
                msg.To.Add(MailboxAddress.Parse(to));
                msg.Subject = subject;
                msg.Body = new TextPart("html") { Text = htmlBody };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, useSsl: true);
                await smtp.AuthenticateAsync(user, pass);
                await smtp.SendAsync(msg);
                await smtp.DisconnectAsync(quit: true);

                _logger.LogInformation("Email sent to {Email} via SMTP {Host}:{Port}", to, host, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                // не перебрасываем дальше – иначе любой сбой SMTP даст 500
            }
        }
    }
}
