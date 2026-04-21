using JobPortal.Application.Common;
using JobPortal.Application.Common.Exceptions;
using JobPortal.Application.Common.Models;
using JobPortal.Application.DTOs;
using JobPortal.Application.Interfaces;
using JobPortal.Domain;

namespace JobPortal.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepo;
    private readonly IJobApplicationRepository _applicationRepo;
    private readonly IFileStorageService _fileStorage;
    private readonly FileValidator _fileValidator;

    public JobService(
        IJobRepository jobRepo, 
        IJobApplicationRepository applicationRepo,
        IFileStorageService fileStorage,
        FileValidator fileValidator)
    {
        _jobRepo = jobRepo;
        _applicationRepo = applicationRepo;
        _fileStorage = fileStorage;
        _fileValidator = fileValidator;
    }
    
    public async Task<JobResponse> CreateJob(CreateJobRequest request, string userId)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = request.Title!,
            Description = request.Description!,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _jobRepo.CreateAsync(job);

        return new JobResponse(
            created.Id, 
            created.Title, 
            created.Description, 
            created.CreatedBy
            );
    }

    public async Task<PagedResult<JobResponse>> GetJobs(PaginationParams param)
    {
        var (items, total) = await _jobRepo.GetPagedAsync(param);

        return new PagedResult<JobResponse>
        {
            Total = total,
            Data = items.Select(j => new JobResponse
            (
                j.Id, 
                j.Title, 
                j.Description, 
                j.CreatedBy
            ))
            .ToList()
        };
    }

    public async Task ApplyJob(Guid jobId, string userId, Stream fileStream, string fileName)
    {

        var job = await _jobRepo.GetByIdAsync(jobId)
            ?? throw new NotFoundException("Job not found");

        var alreadyApplied = await _applicationRepo.HasAppliedAsync(jobId, userId);
        if (alreadyApplied)
            throw new InvalidOperationAppException("You have already applied to this job");

        _fileValidator.Validate(fileName, fileStream.Length);

        var resumeUrl = await _fileStorage.SaveResumeAsync(fileStream, fileName);

        await _applicationRepo.CreateAsync(new JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            UserId = userId,
            ResumeUrl = resumeUrl,
            AppliedAt = DateTime.UtcNow
        });
    }
}
