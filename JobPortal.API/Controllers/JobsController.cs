using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JobPortal.Application.DTOs;
using JobPortal.Application.Interfaces;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : BaseController
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [Authorize(Roles = "Admin,Recruiter")]
    [HttpPost]
    public async Task<IActionResult> CreateJob(CreateJobRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var job = await _jobService.CreateJob(request, userId);

        return Ok(job);
    }

    /// <summary>
    /// Get jobs with advanced filtering, sorting, and pagination
    /// </summary>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/jobs?page=1&amp;pageSize=10
    ///     GET /api/jobs?search=developer&amp;location=bangalore
    ///     GET /api/jobs?jobType=FullTime&amp;experienceLevel=MidLevel
    ///     GET /api/jobs?minSalary=50000&amp;maxSalary=100000
    ///     GET /api/jobs?sortBy=salary&amp;order=desc
    ///     GET /api/jobs?skills=react,node&amp;location=mumbai
    /// 
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] JobQueryParams queryParams)
    {
        var result = await _jobService.GetJobs(queryParams);
        return Ok(result);
    }

    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetJobById(Guid jobId)
    {
        var job = await _jobService.GetJobById(jobId);
        if (job == null)
            return NotFound();

        return Ok(job);
    }

    [Authorize(Roles = "User")]
    [HttpPost("{jobId}/apply")]
    public async Task<IActionResult> Apply(Guid jobId, IFormFile resume)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        if (resume == null || resume.Length == 0)
            return BadRequest(new { message = "Resume file is required" });

        using var stream = new MemoryStream();
        await resume.CopyToAsync(stream);
        stream.Position = 0;
            
        await _jobService.ApplyJob(jobId, userId, stream, resume.FileName);

        return Ok("Applied successfully");
    }

}

