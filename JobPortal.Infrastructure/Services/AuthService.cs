using JobPortal.Application.Interfaces;
using JobPortal.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using JobPortal.Application.Common.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using JobPortal.Infrastructure.Persistence;
using JobPortal.Application.Common.Exceptions;
using JobPortal.Application.Common;
using Microsoft.Extensions.Configuration;

namespace JobPortal.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwt;
    private readonly AppDbContext _context;
     private readonly IEmailService _emailService;
    private readonly string _appUrl;

    public AuthService(
        UserManager<AppUser> userManager, 
        IOptions<JwtSettings> options, 
        AppDbContext context, 
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwt = options.Value;
        _context = context;
        _emailService = emailService;
        _appUrl = configuration["AppUrl"]!;
    }

    public async Task<Result<string>> Register(string email, string password)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<string>.Fail(errors);
        }

        await _userManager.AddToRoleAsync(user, "User");

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Encode the token — it can contain special characters
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(email);

        var verificationLink = $"{_appUrl}/api/auth/verify-email?email={encodedEmail}&token={encodedToken}";

        // Send verification email
        await _emailService.SendEmailVerificationAsync(email, verificationLink);

        return Result<string>.Ok("Registration successful. Please check your email to verify your account.");
    }

    public async Task<Result<object>> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return Result<object>.Unauthorized("Invalid credentials");

        if (!user.IsEmailVerified)
            return Result<object>.Fail("Please verify your email before logging in.", 403);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateRefreshToken(),
            ExpiryDate = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Result<object>.Ok(new { accessToken, refreshToken = refreshToken.Token });
    }

    public async Task<Result<string>> VerifyEmail(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
            return Result<string>.NotFound("User not found");

        if (user.IsEmailVerified)
            return Result<string>.Ok("Email already verified. You can log in.");

        // Decode the token back before passing to Identity
        var decodedToken = Uri.UnescapeDataString(token);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return Result<string>.Fail("Invalid or expired verification link.");

        // Mark our custom flag as well
        user.IsEmailVerified = true;
        await _userManager.UpdateAsync(user);

        return Result<string>.Ok("Email verified successfully. You can now log in.");
    }

    public async Task<Result<string>> Refresh(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (token == null || token.User == null || token.IsRevoked || token.ExpiryDate < DateTime.UtcNow)
            return Result<string>.Unauthorized("Invalid refresh token");

        var roles = await _userManager.GetRolesAsync(token.User);

        var newAccessToken = GenerateJwtToken(token.User, roles);

        return Result<string>.Ok(newAccessToken);
    }

    public Task<Result<string>> Secure()
        => Task.FromResult(Result<string>.Ok("You are authenticated"));
    public Task<Result<string>> AdminOnly()
        => Task.FromResult(Result<string>.Ok("You are admin and authenticated"));

    private string GenerateJwtToken(AppUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

}
