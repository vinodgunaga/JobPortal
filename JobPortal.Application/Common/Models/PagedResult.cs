using System;

namespace JobPortal.Application.Common.Models;

public class PagedResult<T>
{
    public int Total { get; set; }

    public List<T>? Data { get; set; }
}
