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

    [HttpGet]
    public async Task<IActionResult> GetJobs([FromQuery] PaginationParams param)
    {
        return Ok(await _jobService.GetJobs(param));
    }

    [Authorize(Roles = "User")]
    [HttpPost("{jobId}/apply")]
    public async Task<IActionResult> Apply(Guid jobId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
            
        await _jobService.ApplyJob(jobId, userId);

        return Ok("Applied successfully");
    }

}

