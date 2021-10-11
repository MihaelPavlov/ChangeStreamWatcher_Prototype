namespace ChangeStreamWatcher_Blazor.Services.Enrichment
{
    /// <summary>
    /// A default implementation of the <see cref="ILogEnricher"/> interface that does nothing.
    /// It should be used when the process does not need to enrich the logs.
    /// </summary>
    public class EmptyEnricher : ILogEnricher
    {
        /// <inheritdoc />
        public void Enrich(ILogDocument document)
        {
        }
    }
}
