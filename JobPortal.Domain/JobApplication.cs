namespace JobPortal.Domain;

public class JobApplication
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public Guid JobId { get; set; }

    public string? ResumeUrl { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }

    public Job? Job { get; set; }
}
