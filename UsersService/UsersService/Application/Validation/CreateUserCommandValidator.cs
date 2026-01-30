using FluentValidation;
using UsersService.Application.Commands;

namespace UsersService.Application.Validation
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
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
