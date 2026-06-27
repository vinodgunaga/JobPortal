namespace JobPortal.Application.Common.Settings;

public class EmailSettings
{
    public string FromAddress { get; set; } = null!;
    
    public string FromName { get; set; } = null!;

    public string ResendApiKey { get; set; } = null!;
}