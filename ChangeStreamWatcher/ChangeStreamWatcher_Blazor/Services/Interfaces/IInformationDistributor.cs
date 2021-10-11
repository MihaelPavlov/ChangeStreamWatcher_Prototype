using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Services.Interfaces
{
    /// <summary>
    /// An interface that should be used to distribute to changes related to the tenant execution context or to trigger those changes.
    /// </summary>
    /// <typeparam name="TKey">The type of unique identifier used among our databases.</typeparam>
    /// <remarks>
    /// Preferred way of using the interface:
    /// - Accept it from constructor and subscribe for tenant information changes using the following construction: `tenantInformationDistributor.DistributeTo(this)`
    /// </remarks>
    public interface IInformationDistributor<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        /// This method should be used in order to "subscribe" the passed <paramref name="consumer"/> to tenant context changes.
        /// </summary>
        /// <param name="consumer">The <see cref="InformationConsumer{TKey}"/> that should be subscribed to changes.</param>
        void DistributeTo(IInformationConsumer<TKey> consumer);

        /// <summary>
        /// Use this method to change (or initially set) the tenant context for the current operation.
        /// </summary>
        /// <param name="information">A <see cref="Information{TKey}"/> defining the new tenant context.</param>
        void Provide(Information<TKey> information);
    }
}
