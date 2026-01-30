using FluentValidation;

namespace PostsService.Application.Validation
{
    public sealed class ValidatorRunner : IValidatorRunner
    {
        private readonly IServiceProvider _provider;

        public ValidatorRunner(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task ValidateAsync<T>(T instance, CancellationToken ct = default)
        {
            var validator = _provider.GetService<IValidator<T>>();

            if (validator is null)
            {
                return;
            }

            var result = await validator.ValidateAsync(instance, ct);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
