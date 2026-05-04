using JobPortal.Domain;

namespace JobPortal.Tests.Helpers;

/// <summary>
/// Helper class to create test data quickly
/// </summary>
public static class TestDataHelper
{
    public static Job CreateJob(
        string title = "Test Job",
        string description = "Test Description",
        string createdBy = "test-user-id")
    {
        return new Job
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<Job> CreateMultipleJobs(int count)
    {
        var jobs = new List<Job>();
        for (int i = 1; i <= count; i++)
        {
            jobs.Add(new Job
            {
                Id = Guid.NewGuid(),
                Title = $"Job {i}",
                Description = $"Description for job {i}",
                CreatedBy = "test-user",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        return jobs;
    }

    public static JobApplication CreateApplication(Guid jobId, string userId)
    {
        return new JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            UserId = userId,
            ResumeUrl = "/uploads/test-resume.pdf",
            AppliedAt = DateTime.UtcNow
        };
    }
}
