using System.ComponentModel.DataAnnotations;

namespace ERP.Banking.Application.Settings;

/// <summary>
/// JWT configuration bound from appsettings.json → "JwtSettings".
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required, MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    /// <summary>Access token lifetime in minutes. Default: 60.</summary>
    [Range(1, 1440)]
    public int ExpirationMinutes { get; init; } = 60;

    /// <summary>Refresh token lifetime in days. Default: 7.</summary>
    [Range(1, 90)]
    public int RefreshTokenDays { get; init; } = 7;
}