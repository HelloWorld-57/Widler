namespace UsersService.Application.Exceptions
{
    public sealed class ForbiddenException : Exception
    {
        public ForbiddenException()
            : base("You are not allowed to perform this operation.")
        {
        }
    }
}
