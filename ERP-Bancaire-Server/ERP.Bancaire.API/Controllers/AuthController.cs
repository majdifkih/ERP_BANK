using ERP.Bancaire.Application.Interfaces;
using ERP.Bancaire.Application.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Bancaire.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result =
            await _authService.LoginAsync(request);

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request)
    {
        var result =
            await _authService.RefreshTokenAsync(
                request.RefreshToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);

        return NoContent();
    }
}