namespace ChangeStreamWatcher_Blazor.Services
{
    public interface ILogEnricher
    {
        void Enrich(ILogDocument document);
    }
}