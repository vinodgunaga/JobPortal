namespace JobPortal.Domain;

public class RefreshToken
{
    public Guid Id { get; set; }

    public required string UserId { get; set; }

    public required string Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsRevoked { get; set; }

    public AppUser? User { get; set; }
}
