using JobPortal.Application.Common.Models;
using JobPortal.Application.DTOs;

namespace JobPortal.Application.Interfaces;

public interface IJobService
{
    Task<JobResponse> CreateJob(CreateJobRequest request, string userId);

    Task<PagedResult<JobResponse>> GetJobs(PaginationParams param);
    
    Task ApplyJob(Guid jobId, string userId, Stream fileStream, string fileName);
}
