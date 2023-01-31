namespace Rydo.HttpClient.Configurations
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpServicesSettingsExtension
    {
        public static IServiceCollection AddHttpServicesSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            Helper.GuardNull(configuration, nameof(configuration));

            var serviceEntriesContext = services.TryGetServiceEntries(configuration);
            
            var endpointEntriesContext = services.TryGetEndpointEntriesDefinition(configuration, serviceEntriesContext);
           
            services.AddSingleton(serviceEntriesContext);
            services.AddSingleton(endpointEntriesContext);
            
            return services;
        }
    }
}