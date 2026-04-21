using System;

namespace JobPortal.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string verificationLink);
}
