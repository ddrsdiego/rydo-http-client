namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Exceptions;

    public interface IServiceEntriesContext
    {
        ImmutableDictionary<string, ServiceEntry> Entries { get; }

        void AddNewServiceEntry(IEnumerable<ServiceEntry> serviceEntries);
    }
    
    public class ServiceEntriesContext : IServiceEntriesContext
    {
        public ServiceEntriesContext()
        {
            Entries = ImmutableDictionary<string, ServiceEntry>.Empty;
        }

        public ImmutableDictionary<string, ServiceEntry> Entries { get; private set; }

        public void AddNewServiceEntry(IEnumerable<ServiceEntry> serviceEntries)
        {
            if (serviceEntries == null) throw new ArgumentNullException(nameof(serviceEntries));
            
            if (!serviceEntries.Any()) throw new ArgumentNullException(nameof(serviceEntries));

            foreach (var serviceEntry in serviceEntries)
            {
                AddNewServiceEntry(serviceEntry);
            }
        }

        private void AddNewServiceEntry(ServiceEntry serviceEntry)
        {
            Helper.GuardNull(serviceEntry);
            Helper.GuardNull(serviceEntry.BaseAddress);
            Helper.GuardNull(serviceEntry.Name);
            Helper.GuardNull(serviceEntry.RepositoryName);

            if (Entries.TryGetValue(serviceEntry.Name, out _))

                throw new ServiceEntryAlreadyExistsException(serviceEntry.Name);
            Entries = Entries.Add(serviceEntry.Name, serviceEntry);
        }
    }
}