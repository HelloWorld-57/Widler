using FluentValidation;
using UsersService.Application.Commands;

namespace UsersService.Application.Validation
{
    public sealed class ReplaceUserCommandValidator
    : AbstractValidator<ReplaceUserCommand>
    {
        public ReplaceUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.SecondName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.BirthDate)
                .NotEmpty();
        }
    }
}
