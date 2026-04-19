namespace JobPortal.Domain;

public class Company
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string CreatedBy { get; set; }

    public AppUser? User { get; set; }
}
