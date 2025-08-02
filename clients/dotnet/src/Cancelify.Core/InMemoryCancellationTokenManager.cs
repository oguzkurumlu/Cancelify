using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Cancelify.Core
{
    public class InMemoryCancellationTokenManager : IDistributedCancellationToken
    {
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources = new ConcurrentDictionary<string, CancellationTokenSource>();

        public CancellationToken GetToken(string id)
        {
            return _tokenSources.GetOrAdd(id, _ => new CancellationTokenSource()).Token;
        }

        public Task CancelAsync(string id)
        {
            if (_tokenSources.TryRemove(id, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var tokenSource in _tokenSources)
            {
                tokenSource.Value.Dispose();
            }
        }
    }
}
