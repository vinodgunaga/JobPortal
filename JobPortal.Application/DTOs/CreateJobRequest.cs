using JobPortal.Domain;

namespace JobPortal.Application.DTOs;

public record CreateJobRequest(
    string Title, 
    string Description,
    string? Location = null,
    JobType JobType = JobType.FullTime,
    ExperienceLevel ExperienceLevel = ExperienceLevel.MidLevel,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? Company = null,
    DateTime? Deadline = null,
    string? Skills = null // Comma-separated
);
