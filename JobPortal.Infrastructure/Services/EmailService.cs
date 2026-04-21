using JobPortal.Application.Interfaces;
using JobPortal.Application.Common.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JobPortal.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string verificationLink)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "Verify your JobPortal email";

        // Build a clean HTML email body
        message.Body = new TextPart("html")
        {
            Text = $"""
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

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}
