using System;

namespace JobPortal.Application.DTOs;

public record PaginationParams(
    int Page = 1, 
    int PageSize = 10, 
    string? Title = null
);