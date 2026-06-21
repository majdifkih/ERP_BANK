namespace ERP.Banking.Application.Interfaces.Email;

/// <summary>
/// Abstracts transactional email delivery.
/// Implemented in the Infrastructure layer via MailKit/SMTP.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an HTML email to the specified recipient.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">HTML body content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}