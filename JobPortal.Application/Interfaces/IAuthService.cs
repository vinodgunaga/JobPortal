using JobPortal.Application.Common;

namespace JobPortal.Application.Interfaces;

public interface IAuthService
{
    Task<Result<string>> Register(string email, string password);

    Task<Result<object>> Login(string email, string password);

    Task<Result<string>> Refresh(string refreshToken);

    Task<Result<string>> Secure();

    Task<Result<string>> AdminOnly();
}
