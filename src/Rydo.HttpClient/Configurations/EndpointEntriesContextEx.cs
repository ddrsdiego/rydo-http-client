namespace Rydo.HttpClient.Configurations
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class EndpointEntriesContextEx
    {
        internal const string NoDefinedClientException = "There are no definitions for client entries";

        public static IEndpointEntriesContext TryGetEndpointEntriesDefinition(this IServiceCollection services,
            IConfiguration configuration, IServiceEntriesContext serviceEntries)
        {
            var httpPoliciesEntry = services.TryGetHttpPoliciesEntry(configuration);

            var clientEntries = configuration.GetSection(Constants.ClientEntrySection).Get<ClientEntry[]>();
            if (clientEntries is null)
                throw new InvalidOperationException(NoDefinedClientException);

            return TryGetClientEntries(x => x.AddEndpointEntry(serviceEntries, clientEntries, httpPoliciesEntry));
        }

        public static IEndpointEntriesContext TryGetClientEntries(Action<EndpointEntriesContext> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            var endpointEntryDefinition = new EndpointEntriesContext();
            definitions(endpointEntryDefinition);

            return endpointEntryDefinition;
        }
    }
}