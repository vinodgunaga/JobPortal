using System;
using JobPortal.Application.Common.Models;
using JobPortal.Application.DTOs;
using JobPortal.Domain;

namespace JobPortal.Application.Interfaces;

public interface IJobService
{
    Task<JobResponse> CreateJob(CreateJobRequest request, string userId);

    Task<PagedResult<JobResponse>> GetJobs(PaginationParams param);
    
    Task ApplyJob(Guid jobId, string userId);
}
