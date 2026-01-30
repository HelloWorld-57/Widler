using FluentValidation;
using PostsService.Application.Commands;

namespace PostsService.Application.Validation
{
    public sealed class ReplacePostCommandValidator
    : AbstractValidator<ReplacePostCommand>
    {
        public ReplacePostCommandValidator()
        {
            RuleFor(x => x.PostId)
                .NotEmpty();

            RuleFor(x => x.Caption)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Content)
                .NotEmpty();
        }
    }
}
