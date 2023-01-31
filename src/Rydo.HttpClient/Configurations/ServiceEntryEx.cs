namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceEntryEx
    {
        internal const string NoServiceConfigurationException = "There are no definitions for service entries";

        public static IServiceEntriesContext TryGetServiceEntries(this IServiceCollection services,
            IConfiguration configuration)
        {
            var serviceEntries = configuration.GetSection(Constants.ServicesSection)
                .Get<ServiceEntry[]>();

            if (serviceEntries is null || 
                !serviceEntries.Any())
                throw new InvalidOperationException(NoServiceConfigurationException);

            return TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries));
        }

        public static IServiceEntriesContext TryGetServiceEntries(Action<IServiceEntriesContext> definitions)
        {
            var serviceEntryDefinition = new ServiceEntriesContext();
            definitions(serviceEntryDefinition);

            return serviceEntryDefinition;
        }
    }
}