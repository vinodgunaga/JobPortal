using FluentValidation;
using JobPortal.Application.DTOs;

namespace JobPortal.Application.Validators;

public class CreateJobRequestValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required");
    }
}
