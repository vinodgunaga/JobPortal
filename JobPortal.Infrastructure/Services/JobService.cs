using JobPortal.Application.Common;
using JobPortal.Application.Common.Exceptions;
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
    
    public async Task<Job> CreateJob(CreateJobRequest request, string userId)
    {
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Title = request.Title!,
            Description = request.Description!,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            Location = request.Location,
            JobType = request.JobType,
            ExperienceLevel = request.ExperienceLevel,
            MinSalary = request.MinSalary,
            MaxSalary = request.MaxSalary,
            Company = request.Company,
            Deadline = request.Deadline,
            Skills = request.Skills,
            IsActive = true
        };

        return await _jobRepo.CreateAsync(job);       
    }

    public async Task<PagedResult<JobResponse>> GetJobs(JobQueryParams queryParams)
    {
        var pagedJobs = await _jobRepo.GetPagedAsync(queryParams);

        return new PagedResult<JobResponse>
        {
            Items = pagedJobs.Items.Select(MapToResponse).ToList(),
            TotalCount = pagedJobs.TotalCount,
            Page = pagedJobs.Page,
            PageSize = pagedJobs.PageSize
        };
    }

    // Keep old method for backward compatibility
    public async Task<object> GetJobs(PaginationParams param)
    {
        var (items, total) = await _jobRepo.GetPagedAsync(param);

        return new
        {
            Total = total,
            Data = items.Select(MapToResponse).ToList()
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

    private static JobResponse MapToResponse(Job job)
    {
        return new JobResponse(
            job.Id,
            job.Title,
            job.Description,
            job.CreatedBy,
            job.CreatedAt,
            job.Location,
            job.JobType,
            job.ExperienceLevel,
            job.MinSalary,
            job.MaxSalary,
            job.Company,
            job.IsActive,
            job.Deadline,
            job.Skills
        );
    }
}
