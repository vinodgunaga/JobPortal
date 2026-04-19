namespace JobPortal.Application.DTOs;

public record CreateJobRequest(
    string Title, 
    string Description
);
