namespace ChangeStreamWatcher_Blazor.Services.Interfaces
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// An interface that should be used to access the current execution (tenant) context of the operation.
    /// </summary>
    /// <typeparam name="TKey">The type of unique identifier used among our databases.</typeparam>
    public interface IInformationProvider<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Gets the <see cref="Information{TKey}"/> distributed to all consumers and determining the execution (tenant) context of the operation.
        /// </summary>
        [NotNull]
        Information<TKey> Information { get; }
    }
}
