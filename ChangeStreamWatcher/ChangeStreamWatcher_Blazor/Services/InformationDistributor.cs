using ChangeStreamWatcher_Blazor.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChangeStreamWatcher_Blazor.Services
{
    public class InformationDistributor<TKey> : IInformationDistributor<TKey>, IInformationProvider<TKey>
       where TKey : struct, IEquatable<TKey>
    {
        private readonly List<IInformationConsumer<TKey>> _consumers = new List<IInformationConsumer<TKey>>();

        public Information<TKey> Information { get; private set; }

        public void DistributeTo(IInformationConsumer<TKey> consumer)
        {
            if (consumer is null)
                return;

            if (this.Information != null)
                consumer.Consume(this.Information);

            this._consumers.Add(consumer);
        }

        public void Provide(Information<TKey> information)
        {
            if (information is null)
                return;

            this.Information = information;
            foreach (var consumer in this._consumers)
                consumer.Consume(information);
        }
    }
}
