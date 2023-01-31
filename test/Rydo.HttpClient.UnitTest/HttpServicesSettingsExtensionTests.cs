namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Configurations;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class HttpServicesSettingsExtensionTests
    {
        [Fact]
        public void AddHttpServicesSettings_NoConfiguration_ThrowException()
        {
            const string message = "Value cannot be null. (Parameter 'configuration')";

            var services = new ServiceCollection();
            Action action = () => services.AddHttpServicesSettings(null);

            var exception = Assert.Throws<ArgumentNullException>(action);
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void AddHttpServicesSettings_TestBinding()
        {
            var services = new ServiceCollection();
            services.AddHttpServicesSettings(CreateValidConfiguration());

            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IServiceEntriesContext>().Should().NotBeNull();
            provider.GetRequiredService<IEndpointEntriesContext>().Should().NotBeNull();
        }

        private static IConfiguration CreateValidConfiguration()
        {
            var dicConfig = new Dictionary<string, string>
            {
                { "Mode", "MemoryConfigProvider" },
                { "HttpServices:Services:0:Name", "products" },
                { "HttpServices:Services:0:RepositoryName", "products" },
                { "HttpServices:Services:0:BaseAddress", "http://localhost:5010" },
                { "HttpServices:Services:1:Name", "customers" },
                { "HttpServices:Services:1:RepositoryName", "customers" },
                { "HttpServices:Services:1:BaseAddress", "http://localhost:5020" },

                { "HttpServices:Clients:0:Service", "products" },
                { "HttpServices:Clients:0:TimeoutInSeconds", "1" },
                { "HttpServices:Clients:0:Endpoints:0:Name", "product" },
                { "HttpServices:Clients:0:Endpoints:0:Path", "/api/product/{id}" },
                { "HttpServices:Clients:0:Endpoints:0:Retry:NumberOfRetries", "1" },
                { "HttpServices:Clients:0:Endpoints:0:Retry:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:Clients:0:Endpoints:0:Retry:RetryProgressionBaseValueInSeconds", "2" },
                { "HttpServices:Clients:0:Endpoints:1:Name", "product-prices" },
                { "HttpServices:Clients:0:Endpoints:1:Path", "/api/product/{id}/prices" },
                { "HttpServices:Clients:0:Endpoints:1:Retry:NumberOfRetries", "1" },
                { "HttpServices:Clients:0:Endpoints:1:Retry:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:Clients:0:Endpoints:1:Retry:RetryProgressionBaseValueInSeconds", "2" },

                { "HttpServices:Clients:1:Service", "customers" },
                { "HttpServices:Clients:1:TimeoutInSeconds", "10" },
                { "HttpServices:Clients:1:Endpoints:0:Name", "customers" },
                { "HttpServices:Clients:1:Endpoints:0:Path", "/api/customers/{id}" },
                { "HttpServices:Clients:1:Endpoints:0:Retry:NumberOfRetries", "1" },
                { "HttpServices:Clients:1:Endpoints:0:Retry:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:Clients:1:Endpoints:0:Retry:RetryProgressionBaseValueInSeconds", "2" },
                { "HttpServices:Clients:1:Endpoints:1:Name", "customers-suitability" },
                { "HttpServices:Clients:1:Endpoints:1:Path", "/api/customers/{id}/suitability" },
                { "HttpServices:Clients:1:Endpoints:1:Retry:NumberOfRetries", "1" },
                { "HttpServices:Clients:1:Endpoints:1:Retry:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:Clients:1:Endpoints:1:Retry:RetryProgressionBaseValueInSeconds", "2" },

                { "HttpServices:HttpPolicies:Default:NumberOfRetries", "1" },
                { "HttpServices:HttpPolicies:Default:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:HttpPolicies:Default:RetryProgressionBaseValueInSeconds", "2" },
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dicConfig)
                .Build();

            return configuration;
        }
    }
}