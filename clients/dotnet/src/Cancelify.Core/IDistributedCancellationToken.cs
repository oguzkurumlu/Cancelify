namespace Cancelify.Core
{
    public interface IDistributedCancellationToken
    {
        CancellationToken GetToken(string id);
        Task CancelAsync(string id);
    }
}
