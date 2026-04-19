using JobPortal.Application.DTOs;
using JobPortal.Application.Interfaces;
using JobPortal.Domain;
using JobPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Job> CreateAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<(IEnumerable<Job> items, int total)> GetPagedAsync(PaginationParams param)
    {
        var query = _context.Jobs.AsQueryable();

        if (!string.IsNullOrEmpty(param.Title))
            query = query.Where(x => x.Title.Contains(param.Title));

        var total = await query.CountAsync();

        var items = await query
            .Skip((param.Page - 1) * param.PageSize)
            .Take(param.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Job?> GetByIdAsync(Guid id)
        => await _context.Jobs.FindAsync(id);
}
