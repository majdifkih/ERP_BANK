using ERP.Bancaire.Domain.Entities;

namespace ERP.Bancaire.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);

    Task<LoginResponse> RefreshTokenAsync(string refreshToken);

    Task LogoutAsync(string refreshToken);
}