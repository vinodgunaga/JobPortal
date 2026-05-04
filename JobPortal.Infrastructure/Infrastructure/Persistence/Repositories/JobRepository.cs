using JobPortal.Application.DTOs;
using JobPortal.Application.Interfaces;
using JobPortal.Domain;
using JobPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

    public async Task<PagedResult<Job>> GetPagedAsync(JobQueryParams queryParams)
    {
        var query = _context.Jobs.AsQueryable();

        // Apply filters
        query = ApplyFilters(query, queryParams);

        // Get total count after filtering
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, queryParams);

        // Apply pagination
        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return new PagedResult<Job>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    /// <summary>
    /// Apply filters to the query based on parameters
    /// </summary>
    private IQueryable<Job> ApplyFilters(IQueryable<Job> query, JobQueryParams queryParams)
    {
        // Search filter - searches across title, description, and company
        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var searchLower = queryParams.Search.ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(searchLower) ||
                j.Description.ToLower().Contains(searchLower) ||
                (j.Company != null && j.Company.ToLower().Contains(searchLower))
            );
        }

        // Location filter
        if (!string.IsNullOrWhiteSpace(queryParams.Location))
        {
            query = query.Where(j => j.Location != null && 
                                   j.Location.ToLower().Contains(queryParams.Location.ToLower()));
        }

        // Job type filter
        if (queryParams.JobType.HasValue)
        {
            query = query.Where(j => j.JobType == queryParams.JobType.Value);
        }

        // Experience level filter
        if (queryParams.ExperienceLevel.HasValue)
        {
            query = query.Where(j => j.ExperienceLevel == queryParams.ExperienceLevel.Value);
        }

        // Salary range filter
        if (queryParams.MinSalary.HasValue)
        {
            query = query.Where(j => j.MaxSalary == null || j.MaxSalary >= queryParams.MinSalary.Value);
        }

        if (queryParams.MaxSalary.HasValue)
        {
            query = query.Where(j => j.MinSalary == null || j.MinSalary <= queryParams.MaxSalary.Value);
        }

        // Company filter
        if (!string.IsNullOrWhiteSpace(queryParams.Company))
        {
            query = query.Where(j => j.Company != null && 
                                   j.Company.ToLower().Contains(queryParams.Company.ToLower()));
        }

        // IsActive filter
        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(j => j.IsActive == queryParams.IsActive.Value);
        }
        else
        {
            // By default, only show active jobs
            query = query.Where(j => j.IsActive);
        }

        // Skills filter - checks if job's skills contain any of the searched skills
        if (!string.IsNullOrWhiteSpace(queryParams.Skills))
        {
            var skills = queryParams.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .Select(s => s.Trim().ToLower())
                                          .ToList();
            
            query = query.Where(j => j.Skills != null && 
                                   skills.Any(skill => j.Skills.ToLower().Contains(skill)));
        }

        // Deadline filter - jobs with deadline after specified date
        if (queryParams.DeadlineAfter.HasValue)
        {
            query = query.Where(j => j.Deadline == null || j.Deadline >= queryParams.DeadlineAfter.Value);
        }

        // Created date filter
        if (queryParams.CreatedAfter.HasValue)
        {
            query = query.Where(j => j.CreatedAt >= queryParams.CreatedAfter.Value);
        }

        return query;
    }

    /// <summary>
    /// Apply sorting to the query based on parameters
    /// </summary>
    private IQueryable<Job> ApplySorting(IQueryable<Job> query, JobQueryParams queryParams)
    {
        if (string.IsNullOrWhiteSpace(queryParams.SortBy))
        {
            // Default sorting: most recent first
            return query.OrderByDescending(j => j.CreatedAt);
        }

        // Dynamic sorting based on property name
        var sortBy = queryParams.SortBy.ToLower();
        var isDescending = queryParams.Order == SortOrder.Desc;

        return sortBy switch
        {
            "title" => isDescending 
                ? query.OrderByDescending(j => j.Title) 
                : query.OrderBy(j => j.Title),
            
            "createdat" or "created" => isDescending 
                ? query.OrderByDescending(j => j.CreatedAt) 
                : query.OrderBy(j => j.CreatedAt),
            
            "location" => isDescending 
                ? query.OrderByDescending(j => j.Location) 
                : query.OrderBy(j => j.Location),
            
            "company" => isDescending 
                ? query.OrderByDescending(j => j.Company) 
                : query.OrderBy(j => j.Company),
            
            "salary" or "minsalary" => isDescending 
                ? query.OrderByDescending(j => j.MinSalary) 
                : query.OrderBy(j => j.MinSalary),
            
            "maxsalary" => isDescending 
                ? query.OrderByDescending(j => j.MaxSalary) 
                : query.OrderBy(j => j.MaxSalary),
            
            "deadline" => isDescending 
                ? query.OrderByDescending(j => j.Deadline) 
                : query.OrderBy(j => j.Deadline),
            
            "jobtype" => isDescending 
                ? query.OrderByDescending(j => j.JobType) 
                : query.OrderBy(j => j.JobType),
            
            "experiencelevel" => isDescending 
                ? query.OrderByDescending(j => j.ExperienceLevel) 
                : query.OrderBy(j => j.ExperienceLevel),
            
            _ => query.OrderByDescending(j => j.CreatedAt) // Default
        };
    }

    // Keep old method for backward compatibility
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
