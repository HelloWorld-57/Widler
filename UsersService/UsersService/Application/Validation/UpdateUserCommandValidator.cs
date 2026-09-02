using FluentValidation;
using UsersService.Application.Commands;

namespace UsersService.Application.Validation
{
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(200)
                .When(x => x.Name is not null);

            RuleFor(x => x.SecondName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(200)
                .When(x => x.SecondName is not null);

            RuleFor(x => x.BirthDate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .When(x => x.BirthDate is not null);

            RuleFor(x => x)
                .Must(x => x.Name is not null || x.SecondName is not null || x.BirthDate is not null)
                .WithMessage("At least one field must be provided");
        }
    }
}
