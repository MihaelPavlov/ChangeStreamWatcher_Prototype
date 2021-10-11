using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Services.Interfaces
{
    using System;

    /// <summary>
    /// An interface that should be used to consume changes related to the tenant execution context or to trigger those changes.
    /// </summary>
    /// <typeparam name="TKey">The type of unique identifier used among our databases.</typeparam>
    public interface IInformationConsumer<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// Use this method to react to changes related to the tenant execution context.
        /// </summary>
        /// <param name="information">A <see cref="Information{TKey}"/> defining the new model context.</param>
        void Consume(Information<TKey> Information);
    }
}
