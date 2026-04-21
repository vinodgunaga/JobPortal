using Microsoft.AspNetCore.Mvc;
using JobPortal.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using JobPortal.Application.Interfaces;

namespace JobPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
        => FromResult(await _authService.Register(request.Email!, request.Password!));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
        => FromResult(await _authService.Login(request.Email!, request.Password!));

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        => FromResult(await _authService.VerifyEmail(email, token));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(string refreshToken)
        => FromResult(await _authService.Refresh(refreshToken));

    [Authorize]
    [HttpGet("secure")]
    public async Task<IActionResult> Secure()
        => FromResult(await _authService.Secure());

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> AdminOnly()
        => FromResult(await _authService.AdminOnly());
}

