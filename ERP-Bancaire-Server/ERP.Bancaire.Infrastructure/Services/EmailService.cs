using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ERP.Bancaire.Application.Interfaces;
using ERP.Bancaire.Application.Settings;

namespace ERP.Bancaire.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        // If SMTP not configured, just log to console
        if (string.IsNullOrWhiteSpace(_settings.Host) || _settings.Port == 0)
        {
            Console.WriteLine($"[EmailService] Email (simulé) vers {toEmail} : {subject}\n{htmlBody}");
            return;
        }

        try
        {
            Console.WriteLine($"[EmailService] Tentative d'envoi à {toEmail} via {_settings.Host}:{_settings.Port}");
            
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                Timeout = 10000
            };

            var from = new MailAddress(_settings.FromEmail, _settings.FromName);
            var mail = new MailMessage()
            {
                From = from,
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            
            Console.WriteLine($"[EmailService] De : {_settings.FromEmail}, À : {toEmail}");
            await client.SendMailAsync(mail);
            Console.WriteLine($"[EmailService] Email envoyé avec succès à {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] ERREUR lors de l'envoi : {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"[EmailService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}