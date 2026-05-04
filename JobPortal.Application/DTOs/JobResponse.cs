using JobPortal.Domain;

namespace JobPortal.Application.DTOs;

public record JobResponse(
    Guid Id, 
    string Title, 
    string Description, 
    string CreatedBy,
    DateTime CreatedAt,
    string? Location = null,
    JobType JobType = JobType.FullTime,
    ExperienceLevel ExperienceLevel = ExperienceLevel.MidLevel,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? Company = null,
    bool IsActive = true,
    DateTime? Deadline = null,
    string? Skills = null
);
