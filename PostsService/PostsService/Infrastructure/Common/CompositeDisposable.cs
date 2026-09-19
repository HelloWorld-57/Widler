namespace PostsService.Infrastructure.Common
{
    public sealed class CompositeDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public CompositeDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            for (var i = _disposables.Length - 1; i >= 0; i--)
            {
                _disposables[i].Dispose();
            }
        }
    }
}
