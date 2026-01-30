namespace UsersService.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityName { get; }
        public string EntityId { get; }

        public NotFoundException(string entityName, string entityId)
            : base($"{entityName} with id '{entityId}' was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}
