namespace JobPortal.Domain;

public class JobApplication
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public Guid JobId { get; set; }

    public AppUser? User { get; set; }

    public Job? Job { get; set; }
}
