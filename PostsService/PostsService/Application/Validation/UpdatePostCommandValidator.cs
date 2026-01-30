using FluentValidation;
using PostsService.Application.Commands;

namespace PostsService.Application.Validation
{
    public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
    {
        public UpdatePostCommandValidator()
        {
            RuleFor(x => x.PostId)
                .NotEmpty();

            RuleFor(x => x.Caption)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(200)
                .When(x => x.Caption is not null);

            RuleFor(x => x.Content)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .When(x => x.Content is not null);

            RuleFor(x => x)
                .Must(x => x.Caption is not null || x.Content is not null)
                .WithMessage("At least one field must be provided");

        }
    }
}
