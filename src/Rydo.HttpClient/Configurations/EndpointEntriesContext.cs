namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Exceptions;

    public interface IEndpointEntriesContext
    {
        ImmutableDictionary<string, EndpointEntry> Entries { get; }

        void AddEndpointEntry(IServiceEntriesContext serviceEntriesContext,
            IEnumerable<ClientEntry> clientEntries,
            HttpPoliciesEntry httpPoliciesEntry);
    }
    
    public class EndpointEntriesContext : IEndpointEntriesContext
    {
        internal const int DefaultTimeout = 30;

        public EndpointEntriesContext()
        {
            Entries = ImmutableDictionary<string, EndpointEntry>.Empty;
        }

        public ImmutableDictionary<string, EndpointEntry> Entries { get; private set; }

        public void AddEndpointEntry(IServiceEntriesContext serviceEntriesContext,
            IEnumerable<ClientEntry> clientEntries,
            HttpPoliciesEntry httpPoliciesEntry)
        {
            if (clientEntries == null) throw new ArgumentNullException(nameof(clientEntries));

            foreach (var clientEntry in clientEntries)
            {
                if (clientEntry.Service is null)
                    continue;

                if (!serviceEntriesContext.Entries.TryGetValue(clientEntry.Service, out var serviceEntry))
                    continue;

                if (clientEntry.Endpoints is null ||
                    !clientEntry.Endpoints.Any())
                    continue;

                Entries = AddEndpointEntry(Entries, serviceEntry, clientEntry, httpPoliciesEntry);
            }
        }

        internal static ImmutableDictionary<string, EndpointEntry> AddEndpointEntry(
            ImmutableDictionary<string, EndpointEntry> entries,
            ServiceEntry serviceEntry,
            ClientEntry clientEntry,
            HttpPoliciesEntry httpPoliciesEntry)
        {
            foreach (var endpointEntry in clientEntry.Endpoints!)
            {
                entries = AddEndpointEntry(entries, serviceEntry, clientEntry, endpointEntry, httpPoliciesEntry);
            }

            return entries;
        }

        internal static ImmutableDictionary<string, EndpointEntry> AddEndpointEntry(
            ImmutableDictionary<string, EndpointEntry> entries,
            ServiceEntry serviceEntry,
            ClientEntry clientEntry,
            EndpointEntry endpointEntry,
            HttpPoliciesEntry httpPoliciesEntry)
        {
            Helper.GuardNull(endpointEntry.Name, nameof(endpointEntry.Name));
            Helper.GuardNull(endpointEntry.Path, nameof(endpointEntry.Path));

            if (entries.TryGetValue(endpointEntry.Name, out _))
                throw new EndpointEntryAlreadyDefinedException(serviceEntry.Name, endpointEntry.Name);

            if (!string.IsNullOrEmpty(endpointEntry.PolicyName) && httpPoliciesEntry.Entries.Any())
            {
                if (!httpPoliciesEntry.Entries.TryGetValue(endpointEntry.PolicyName, out var httpPoliciesItem))
                    throw new PolicyNotDefinedException(endpointEntry.PolicyName);

                UpdateEndpointEntryPolicies(endpointEntry, httpPoliciesItem);
            }

            endpointEntry.BindClient(clientEntry);
            endpointEntry.BindService(serviceEntry);

            return entries.Add(endpointEntry.Name, endpointEntry);
        }

        internal static void UpdateEndpointEntryPolicies(EndpointEntry endpointEntry,
            HttpPoliciesItem? httpPoliciesItem)
        {

            endpointEntry.Retry = endpointEntry.Retry ?? httpPoliciesItem?.Retry;
            endpointEntry.CircuitBreak = endpointEntry.CircuitBreak ?? httpPoliciesItem?.CircuitBreak;

            endpointEntry.TimeoutInSeconds = endpointEntry.TimeoutInSeconds > 0
                ? endpointEntry.TimeoutInSeconds
                : httpPoliciesItem?.TimeoutInSeconds ?? DefaultTimeout;
        }
    }
}