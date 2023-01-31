namespace Rydo.HttpClient.Configurations
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Mime;
    using Filters;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Microsoft.Net.Http.Headers;
    using Policies;
    using Polly;

    public static class HttpServicesConfigurationsExtensions
    {
        internal const string NoEndpointConfigurationException = "There are no definitions for endpoint entries";

        public static IServiceCollection AddHttpServices(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceEntriesContext = BuildEntriesDefinition(services, configuration,
                out var endpointEntriesContext);

            foreach (var (endpointName, endpointEntry) in endpointEntriesContext.Entries)
            {
                services.AddHttpClient(endpointName, client =>
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.ContentType,
                            MediaTypeNames.Application.Json);
                    })
                    .SetHandlerLifetime(TimeSpan.FromMinutes(endpointEntry.HandlerLifetimeInMinutes))
                    .AddTransientHttpErrorPolicy(_ => endpointEntry.GetWaitAndRetryPolicy())
                    .AddTransientHttpErrorPolicy(_ => endpointEntry.GetCircuitBreakPolicy())
                    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(endpointEntry.TimeoutInSeconds))
                    .ConfigurePrimaryHttpMessageHandler(() => endpointEntry.GetHttpMessageHandler());
            }

            services.AddMemoryCache();
            services.AddSingleton(serviceEntriesContext);
            services.AddSingleton(endpointEntriesContext);
            services.AddSingleton<IHttpServiceRequesterFactory, HttpServiceRequesterFactory>();
            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, CustomLoggingFilter>());

            return services;
        }

        internal static IServiceEntriesContext BuildEntriesDefinition(IServiceCollection services,
            IConfiguration configuration,
            out IEndpointEntriesContext endpointEntryContext)
        {
            var serviceEntryDefinition = services.TryGetServiceEntries(configuration);

            endpointEntryContext = services.TryGetEndpointEntriesDefinition(configuration, serviceEntryDefinition);
            if (!endpointEntryContext.Entries.Any())
                throw new InvalidOperationException(NoEndpointConfigurationException);
            return serviceEntryDefinition;
        }
    }
}