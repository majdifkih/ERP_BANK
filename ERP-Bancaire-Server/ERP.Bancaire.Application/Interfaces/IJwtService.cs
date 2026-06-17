using ERP.Bancaire.Domain.Entities;

namespace ERP.Bancaire.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}