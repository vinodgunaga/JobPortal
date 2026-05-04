using JobPortal.Application.DTOs;
using JobPortal.Domain;

namespace JobPortal.Application.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateAsync(Job job);

    /// <summary>
    /// Get paginated, filtered, and sorted jobs
    /// </summary>
    Task<PagedResult<Job>> GetPagedAsync(JobQueryParams queryParams);

    Task<(IEnumerable<Job> items, int total)> GetPagedAsync(PaginationParams param);

    Task<Job?> GetByIdAsync(Guid id);
}