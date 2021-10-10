namespace ChangeStreamWatcher_Blazor.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILogEmitter
    {
        Task EmitBatchAsync(IEnumerable<ILogDocument> documents, CancellationToken cancellationToken);
    }
}