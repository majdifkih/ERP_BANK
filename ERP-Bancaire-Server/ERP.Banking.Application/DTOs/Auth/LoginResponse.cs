namespace ERP.Banking.Application.DTOs.Auth;

/// <summary>
/// Returned after a successful login or token refresh.
/// </summary>
public sealed record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}