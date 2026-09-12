namespace PostsService.Application.Interfaces
{
    public interface ICorrelationContext
    {
        string CorrelationId { get; }
    }
}
