namespace JobPortal.Application.DTOs;

public record JobResponse(
    Guid Id, 
    string Title, 
    string Description, 
    string CreatedBy
);
