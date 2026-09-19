using FluentValidation;
using PostsService.Application.Commands;

namespace PostsService.Application.Validation
{
    public sealed class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        public CreatePostCommandValidator()
        {
            RuleFor(x => x.Caption)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Content)
                .NotEmpty();
        }
    }
}
