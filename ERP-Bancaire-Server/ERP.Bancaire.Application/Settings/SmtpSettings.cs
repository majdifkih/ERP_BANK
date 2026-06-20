namespace ERP.Bancaire.Application.Settings;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
}