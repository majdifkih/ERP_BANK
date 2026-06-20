using System.Threading.Tasks;

namespace ERP.Bancaire.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}