using JobPortal.Application.DTOs;
using JobPortal.Domain;

namespace JobPortal.Application.Interfaces;

public interface IJobRepository
{
    Task<Job> CreateAsync(Job job);

    Task<(IEnumerable<Job> items, int total)> GetPagedAsync(PaginationParams param);

    Task<Job?> GetByIdAsync(Guid id);
}