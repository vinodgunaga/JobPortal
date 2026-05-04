using JobPortal.Application.DTOs;
using JobPortal.Domain;

namespace JobPortal.Application.Interfaces;

public interface IJobService
{
    Task<Job> CreateJob(CreateJobRequest request, string userId);

    Task<PagedResult<JobResponse>> GetJobs(JobQueryParams queryParams);
    
    Task<object> GetJobs(PaginationParams param);
    
    Task ApplyJob(Guid jobId, string userId, Stream fileStream, string fileName);
}
