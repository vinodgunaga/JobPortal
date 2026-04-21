namespace JobPortal.Application.Common.Settings;

public class EmailSettings
{
    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FromAddress { get; set; } = null!;
    
    public string FromName { get; set; } = null!;
}