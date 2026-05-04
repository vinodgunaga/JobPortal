using JobPortal.Domain;

namespace JobPortal.Application.DTOs;

/// <summary>
/// Parameters for querying jobs with pagination, filtering, and sorting
/// </summary>
public record JobQueryParams
{
    // Pagination
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    
    // Sorting
    public string? SortBy { get; init; } // e.g., "createdAt", "title", "salary"
    public SortOrder Order { get; init; } = SortOrder.Desc;
    
    // Search
    public string? Search { get; init; } // Searches in title, description, company
    
    // Filters
    public string? Location { get; init; }
    public JobType? JobType { get; init; }
    public ExperienceLevel? ExperienceLevel { get; init; }
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public string? Company { get; init; }
    public bool? IsActive { get; init; }
    public string? Skills { get; init; } // Comma-separated
    public DateTime? DeadlineAfter { get; init; } // Jobs with deadline after this date
    public DateTime? CreatedAfter { get; init; } // Jobs created after this date
}

public enum SortOrder
{
    Asc,
    Desc
}

