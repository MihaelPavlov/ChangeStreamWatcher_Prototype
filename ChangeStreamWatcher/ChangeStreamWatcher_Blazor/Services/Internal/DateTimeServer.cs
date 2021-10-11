namespace ChangeStreamWatcher_Blazor.Services.Internal
{
    using System;

    using ChangeStreamWatcher_Blazor.Services.Interfaces;

    /// <summary>
    /// Represents an implementation of the <see cref="IDateTimeServer"/> interface.
    /// </summary>
    public class DateTimeServer : IDateTimeServer
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
