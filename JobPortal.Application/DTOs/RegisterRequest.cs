namespace JobPortal.Application.DTOs;

public record RegisterRequest(
    string Email, 
    string Password
);
