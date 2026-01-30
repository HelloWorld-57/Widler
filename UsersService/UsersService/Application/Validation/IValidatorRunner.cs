namespace UsersService.Application.Validation
{
    public interface IValidatorRunner
    {
        Task ValidateAsync<T>(T instance, CancellationToken ct = default);
    }
}
