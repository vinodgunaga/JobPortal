using System;
using JobPortal.Application.Interfaces;
using JobPortal.Domain;
using JobPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Persistence.Repositories;

public class JobApplicationRepository : IJobApplicationRepository
{
     private readonly AppDbContext _context;

    public JobApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(JobApplication application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAppliedAsync(Guid jobId, string userId)
        => await _context.Applications
            .AnyAsync(x => x.JobId == jobId && x.UserId == userId);
}
