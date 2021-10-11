namespace ChangeStreamWatcher_Blazor.Services.Interfaces
{
    using System;

    /// <summary>
    /// Defines an interface for providing a <see cref="DateTimeOffset"/> value.
    /// </summary>
    public interface IDateTimeServer
    {
        DateTimeOffset Now { get; }
    }
}
