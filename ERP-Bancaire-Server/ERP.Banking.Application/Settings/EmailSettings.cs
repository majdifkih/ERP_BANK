using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.Settings;

/// <summary>
/// SMTP configuration bound from appsettings.json → "EmailSettings".
/// </summary>
public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    [Required]
    public string Username { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string SenderName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string SenderEmail { get; init; } = string.Empty;
}