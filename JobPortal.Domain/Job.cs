namespace JobPortal.Domain;

public class Job
{

    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public AppUser? User { get; set; }
}
