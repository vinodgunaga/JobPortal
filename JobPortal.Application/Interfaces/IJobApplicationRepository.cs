using System;
using JobPortal.Domain;

namespace JobPortal.Application.Interfaces;

public interface IJobApplicationRepository
{
    Task CreateAsync(JobApplication application);
    
    Task<bool> HasAppliedAsync(Guid jobId, string userId);
}
