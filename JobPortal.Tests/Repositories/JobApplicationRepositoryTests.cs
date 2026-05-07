using FluentAssertions;
using JobPortal.Domain;
using JobPortal.Infrastructure.Persistence;
using JobPortal.Infrastructure.Persistence.Repositories;
using JobPortal.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Tests.Repositories;

/// <summary>
/// Tests for JobApplicationRepository
/// </summary>
public class JobApplicationRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly JobApplicationRepository _repository;

    public JobApplicationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new JobApplicationRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldAddApplicationToDatabase()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        var application = TestDataHelper.CreateApplication(jobId, userId);

        // Act
        //var result = await _repository.CreateAsync(application);
        await _repository.CreateAsync(application);

        // Assert
        //result.Should().NotBeNull();
        //result.Id.Should().Be(application.Id);
        
        var saved = await _context.Applications.FindAsync(application.Id);
        saved.Should().NotBeNull();
        saved!.JobId.Should().Be(jobId);
        saved.UserId.Should().Be(userId);
    }

    #endregion

    #region HasAppliedAsync Tests

    [Fact]
    public async Task HasAppliedAsync_WhenUserHasApplied_ShouldReturnTrue()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        var application = TestDataHelper.CreateApplication(jobId, userId);
        
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasAppliedAsync(jobId, userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAppliedAsync_WhenUserHasNotApplied_ShouldReturnFalse()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";

        // Act
        var result = await _repository.HasAppliedAsync(jobId, userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAppliedAsync_WhenDifferentUserApplied_ShouldReturnFalse()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var otherUserId = "other-user";
        var application = TestDataHelper.CreateApplication(jobId, otherUserId);
        
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasAppliedAsync(jobId, "user-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasAppliedAsync_WhenSameUserAppliedToDifferentJob_ShouldReturnFalse()
    {
        // Arrange
        var jobId1 = Guid.NewGuid();
        var jobId2 = Guid.NewGuid();
        var userId = "user-123";
        
        var application = TestDataHelper.CreateApplication(jobId1, userId);
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasAppliedAsync(jobId2, userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Multiple Applications

    [Fact]
    public async Task CreateAsync_MultipleApplications_ShouldAllBeSaved()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var app1 = TestDataHelper.CreateApplication(jobId, "user-1");
        var app2 = TestDataHelper.CreateApplication(jobId, "user-2");
        var app3 = TestDataHelper.CreateApplication(jobId, "user-3");

        // Act
        await _repository.CreateAsync(app1);
        await _repository.CreateAsync(app2);
        await _repository.CreateAsync(app3);

        // Assert
        var applications = await _context.Applications
            .Where(a => a.JobId == jobId)
            .ToListAsync();

        applications.Should().HaveCount(3);
    }

    [Fact]
    public async Task HasAppliedAsync_WithMultipleApplications_ShouldCheckCorrectly()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user-123";
        
        // User-123 applied to this job
        var app1 = TestDataHelper.CreateApplication(jobId, userId);
        // Other users also applied
        var app2 = TestDataHelper.CreateApplication(jobId, "user-456");
        var app3 = TestDataHelper.CreateApplication(jobId, "user-789");
        
        _context.Applications.AddRange(app1, app2, app3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasAppliedAsync(jobId, userId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
