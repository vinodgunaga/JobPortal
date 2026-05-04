namespace JobPortal.Domain;

public class Job
{

    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Location { get; set; }
    
    public JobType JobType { get; set; } = JobType.FullTime;
    
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.MidLevel;
    
    public decimal? MinSalary { get; set; }
    
    public decimal? MaxSalary { get; set; }
    
    public string? Company { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? Deadline { get; set; }
    
    public string? Skills { get; set; } // Comma-separated skills
    
    public AppUser? User { get; set; }
}

public enum JobType
{
    FullTime,
    PartTime,
    Contract,
    Internship,
    Freelance
}

public enum ExperienceLevel
{
    EntryLevel,
    MidLevel,
    Senior,
    Lead,
    Executive
}