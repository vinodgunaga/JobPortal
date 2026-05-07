using FluentAssertions;
using JobPortal.Application.DTOs;
using JobPortal.Domain;
using JobPortal.Infrastructure.Persistence;
using JobPortal.Infrastructure.Persistence.Repositories;
using JobPortal.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Tests.Repositories;

/// <summary>
/// Integration tests for JobRepository using in-memory database
/// These test the actual database queries without mocking
/// </summary>
public class JobRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly JobRepository _repository;

    public JobRepositoryTests()
    {
        // Create a unique in-memory database for each test
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new JobRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldAddJobToDatabase()
    {
        // Arrange
        var job = TestDataHelper.CreateJob("DevOps Engineer", "AWS and Docker");

        // Act
        var result = await _repository.CreateAsync(job);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
        
        var savedJob = await _context.Jobs.FindAsync(job.Id);
        savedJob.Should().NotBeNull();
        savedJob!.Title.Should().Be("DevOps Engineer");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnJobWithId()
    {
        // Arrange
        var job = TestDataHelper.CreateJob();

        // Act
        var result = await _repository.CreateAsync(job);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(job);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenJobExists_ShouldReturnJob()
    {
        // Arrange
        var job = TestDataHelper.CreateJob("QA Engineer", "Manual testing");
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(job.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(job.Id);
        result.Title.Should().Be("QA Engineer");
    }

    [Fact]
    public async Task GetByIdAsync_WhenJobDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPageSize()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(15);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(25);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 2,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);

        // Second page should not contain first page items
        var firstPageParam = new JobQueryParams
        {
            Page = 1,
            PageSize = 10
        };
        var firstPageResult = await _repository.GetPagedAsync(firstPageParam);
        
        result.Items.Should().NotIntersectWith(firstPageResult.Items);
    }

    [Fact]
    public async Task GetPagedAsync_WithTitleFilter_ShouldFilterCorrectly()
    {
        // Arrange
        _context.Jobs.AddRange(new[]
        {
            TestDataHelper.CreateJob("React Developer", "Frontend"),
            TestDataHelper.CreateJob("Backend Developer", "Node.js"),
            TestDataHelper.CreateJob("Full Stack Developer", "React and Node"),
        });
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
            {
                Page = 1,
                PageSize = 10,
                Search = "React"
            };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(j =>
        j.Title.Contains("React") ||
        j.Description.Contains("React"));
    }

    [Fact]
    public async Task GetPagedAsync_WithNoResults_ShouldReturnEmpty()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(5);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 1,
            PageSize = 10,
            Search = "NonExistent"
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedAsync_LastPage_ShouldReturnRemainingItems()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(23);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 3,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(3); // 23 total, 10 per page = 3 on last page
        result.TotalCount.Should().Be(23);
    }

    [Fact]
    public async Task GetPagedAsync_PageBeyondTotal_ShouldReturnEmpty()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(5);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 10,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetPagedAsync_TitleFilter_ShouldBeCaseInsensitive()
    {
        // Arrange
        _context.Jobs.Add(TestDataHelper.CreateJob("Python Developer", "Django"));
        await _context.SaveChangesAsync();
        
        var param = new JobQueryParams
            {
                Page = 1,
                PageSize = 10,
                Search = "python"
            };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Title.Should().Be("Python Developer");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetPagedAsync_WithEmptyDatabase_ShouldReturnEmpty()
    {
        // Arrange
        var param = new JobQueryParams
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedAsync_WithPageSizeOne_ShouldReturnOneItem()
    {
        // Arrange
        var jobs = TestDataHelper.CreateMultipleJobs(5);
        _context.Jobs.AddRange(jobs);
        await _context.SaveChangesAsync();

        var param = new JobQueryParams
        {
            Page = 1,
            PageSize = 1
        };

        // Act
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(5);
    }

    #endregion

    #region Multiple Operations

    [Fact]
    public async Task CreateMultipleJobs_ShouldAllBeRetrievable()
    {
        // Arrange & Act
        var job1 = await _repository.CreateAsync(TestDataHelper.CreateJob("Job 1", "Desc 1"));
        var job2 = await _repository.CreateAsync(TestDataHelper.CreateJob("Job 2", "Desc 2"));
        var job3 = await _repository.CreateAsync(TestDataHelper.CreateJob("Job 3", "Desc 3"));

        var param = new JobQueryParams
        {
            Page = 1,
            PageSize = 10
        };
        var result = await _repository.GetPagedAsync(param);

        // Assert
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Items.Should().Contain(j => j.Id == job1.Id);
        result.Items.Should().Contain(j => j.Id == job2.Id);
        result.Items.Should().Contain(j => j.Id == job3.Id);
    }

    #endregion
}
