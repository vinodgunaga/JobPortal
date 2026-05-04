using FluentAssertions;
using JobPortal.Application.Common.Exceptions;
using JobPortal.Application.DTOs;
using JobPortal.Application.Interfaces;
using JobPortal.Domain;
using JobPortal.Infrastructure.Services;
using JobPortal.Tests.Helpers;
using Moq;

namespace JobPortal.Tests.Services;

/// <summary>
/// Tests for JobService - demonstrates mocking repositories
/// </summary>
public class JobServiceTests
{
    private readonly Mock<IJobRepository> _mockJobRepo;
    private readonly Mock<IJobApplicationRepository> _mockApplicationRepo;
    private readonly Mock<IFileStorageService> _mockFileStorage;
    private readonly JobService _service;

    public JobServiceTests()
    {
        _mockJobRepo = new Mock<IJobRepository>();
        _mockApplicationRepo = new Mock<IJobApplicationRepository>();
        _mockFileStorage = new Mock<IFileStorageService>();

        _service = new JobService(
            _mockJobRepo.Object,
            _mockApplicationRepo.Object,
            _mockFileStorage.Object
        );
    }

    #region CreateJob Tests

    [Fact]
    public async Task CreateJob_ShouldCreateJobSuccessfully()
    {
        // Arrange
        var request = new CreateJobRequest("Software Engineer", "Great opportunity");
        var userId = "user-123";

        Job? capturedJob = null;
        _mockJobRepo
            .Setup(x => x.CreateAsync(It.IsAny<Job>()))
            .Callback<Job>(job => capturedJob = job)
            .ReturnsAsync((Job job) => job);

        // Act
        var result = await _service.CreateJob(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Software Engineer");
        result.Description.Should().Be("Great opportunity");
        result.CreatedBy.Should().Be(userId);
        
        capturedJob.Should().NotBeNull();
        capturedJob!.Id.Should().NotBeEmpty();
        capturedJob.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task CreateJob_ShouldCallRepository()
    {
        // Arrange
        var request = new CreateJobRequest("Developer", "Description");
        var userId = "user-123";

        _mockJobRepo
            .Setup(x => x.CreateAsync(It.IsAny<Job>()))
            .ReturnsAsync((Job job) => job);

        // Act
        await _service.CreateJob(request, userId);

        // Assert
        _mockJobRepo.Verify(
            x => x.CreateAsync(It.Is<Job>(j => 
                j.Title == "Developer" && 
                j.CreatedBy == userId
            )), 
            Times.Once
        );
    }

    #endregion

    #region GetJobs Tests

    [Fact]
    public async Task GetJobs_ShouldReturnPagedResults()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(5);
        var paginationParams = new PaginationParams(Page: 1, PageSize: 10);

        _mockJobRepo
            .Setup(x => x.GetPagedAsync(paginationParams))
            .ReturnsAsync((jobs, 5));

        // Act
        var result = await _service.GetJobs(paginationParams);

        // Assert
        result.Should().NotBeNull();
        var resultObj = result as dynamic;
        ((int)resultObj.Total).Should().Be(5);
        ((List<JobResponse>)resultObj.Data).Should().HaveCount(5);
    }

    [Fact]
    public async Task GetJobs_ShouldMapJobsCorrectly()
    {
        // Arrange
        var job = TestDataHelper.CreateJob("Backend Developer", "Node.js position", "user-1");
        var paginationParams = new PaginationParams(Page: 1, PageSize: 10);

        _mockJobRepo
            .Setup(x => x.GetPagedAsync(paginationParams))
            .ReturnsAsync((new List<Job> { job }, 1));

        // Act
        var result = await _service.GetJobs(paginationParams);

        // Assert
        var resultObj = result as dynamic;
        var data = (List<JobResponse>)resultObj.Data;
        
        data.Should().HaveCount(1);
        data[0].Id.Should().Be(job.Id);
        data[0].Title.Should().Be("Backend Developer");
        data[0].Description.Should().Be("Node.js position");
        data[0].CreatedBy.Should().Be("user-1");
    }

    #endregion

    #region ApplyJob Tests

    [Fact]
    public async Task ApplyJob_WhenJobNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        using var stream = new MemoryStream();

        _mockJobRepo
            .Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync((Job?)null);

        // Act & Assert
        var act = async () => await _service.ApplyJob(jobId, userId, stream, "resume.pdf");
        
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Job not found");
    }

    [Fact]
    public async Task ApplyJob_WhenAlreadyApplied_ShouldThrowException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        var job = TestDataHelper.CreateJob();
        job.Id = jobId;
        using var stream = new MemoryStream();

        _mockJobRepo
            .Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _mockApplicationRepo
            .Setup(x => x.HasAppliedAsync(jobId, userId))
            .ReturnsAsync(true);

        // Act & Assert
        var act = async () => await _service.ApplyJob(jobId, userId, stream, "resume.pdf");
        
        await act.Should().ThrowAsync<InvalidOperationAppException>()
            .WithMessage("You have already applied to this job");
    }

    [Fact]
    public async Task ApplyJob_WhenValid_ShouldSaveResumeAndCreateApplication()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        var job = TestDataHelper.CreateJob();
        job.Id = jobId;
        
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var fileName = "john-doe-resume.pdf";
        var expectedResumeUrl = "/uploads/john-doe-resume.pdf";

        _mockJobRepo
            .Setup(x => x.GetByIdAsync(jobId))
            .ReturnsAsync(job);

        _mockApplicationRepo
            .Setup(x => x.HasAppliedAsync(jobId, userId))
            .ReturnsAsync(false);

        _mockFileStorage
            .Setup(x => x.SaveResumeAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync(expectedResumeUrl);

        // Act
        await _service.ApplyJob(jobId, userId, stream, fileName);

        // Assert - Verify file was saved
        _mockFileStorage.Verify(
            x => x.SaveResumeAsync(It.IsAny<Stream>(), fileName),
            Times.Once,
            "Resume file should be saved"
        );

        // Assert - Verify application was created
        _mockApplicationRepo.Verify(
            x => x.CreateAsync(It.Is<JobApplication>(app =>
                app.JobId == jobId &&
                app.UserId == userId &&
                app.ResumeUrl == expectedResumeUrl
            )),
            Times.Once,
            "Job application should be created with correct data"
        );
    }

    [Fact]
    public async Task ApplyJob_ShouldValidateFileBeforeSaving()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        var job = TestDataHelper.CreateJob();
        job.Id = jobId;
        
        using var stream = new MemoryStream();
        var fileName = "resume.pdf";

        _mockJobRepo.Setup(x => x.GetByIdAsync(jobId)).ReturnsAsync(job);
        _mockApplicationRepo.Setup(x => x.HasAppliedAsync(jobId, userId)).ReturnsAsync(false);
        _mockFileStorage.Setup(x => x.SaveResumeAsync(It.IsAny<Stream>(), fileName))
            .ReturnsAsync("/uploads/resume.pdf");

        // Act
        await _service.ApplyJob(jobId, userId, stream, fileName);

        // Assert
        _mockFileStorage.Verify(
            x => x.SaveResumeAsync(It.IsAny<Stream>(), fileName),
            Times.Once
        );
    }

    #endregion
}
