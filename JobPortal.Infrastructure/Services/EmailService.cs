using JobPortal.Application.Interfaces;
using JobPortal.Application.Common.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace JobPortal.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public EmailService(HttpClient httpClient, IOptions<EmailSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }
    public async Task SendEmailVerificationAsync(string toEmail, string verificationLink)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ResendApiKey);

        var payload = new
        {
            from = $"{_settings.FromName} <{_settings.FromAddress}>",
            to = new[] { toEmail },
            subject = "Verify your JobPortal email",
            html = $"""
                <h2>Welcome to JobPortal!</h2>
                <p>Please verify your email address by clicking the button below.</p>
                <p>This link expires in 24 hours.</p>
                <a href="{verificationLink}" 
                style="background:#2563eb;color:white;padding:12px 24px;
                        text-decoration:none;border-radius:6px;display:inline-block">
                    Verify Email
                </a>
                <p>Or copy this link: {verificationLink}</p>
                """
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.resend.com/emails", payload);
        response.EnsureSuccessStatusCode();
    }

}
