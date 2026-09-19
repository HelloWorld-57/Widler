namespace UsersService.Application.Interfaces
{
    public interface ICorrelationContext
    {
        string CorrelationId { get; }
    }
}
