using ERP.Bancaire.Domain.Entities;
using ERP.Bancaire.Application.DTOs.Auth;

namespace ERP.Bancaire.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<LoginResponse> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(string refreshToken);
    
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    
    Task ResetPasswordAsync(ResetPasswordRequest request);
}