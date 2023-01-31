namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Configurations;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class HttpServicesConfigurationsExtensionsTests
    {

        private readonly ServiceCollection _services;
        private readonly IConfiguration _configuration;

        public HttpServicesConfigurationsExtensionsTests()
        {
            _services = new ServiceCollection();
            _configuration = CreateValidConfiguration();
        }

        [Fact]
        public void TestInjection()
        {
            _services.AddHttpServices(_configuration);
            var services = _services.BuildServiceProvider();
            var requester = services.GetRequiredService<IHttpServiceRequesterFactory>();
            Assert.NotNull(services.GetRequiredService<IServiceEntriesContext>());
            Assert.NotNull(services.GetRequiredService<IEndpointEntriesContext>());
            Assert.NotNull(requester);
            var client = requester.CreateRequestFor("product");
            Assert.NotNull(client);
        }

        [Fact]
        public void InvalidConfiguration_NoServices_ThrowException()
        {
            void Action() => _services.AddHttpServices(CreateConfigurationWhitoutEndpoint());
            
            var exception = Assert.Throws<InvalidOperationException>((Action) Action);
            HttpServicesConfigurationsExtensions.NoEndpointConfigurationException.Should().Be(exception.Message);
            // Assert.Equal(HttpServicesConfigurationsExtensions.NoEndpointConfigurationException, exception.Message);
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

        private static IConfiguration CreateConfigurationWhitoutEndpoint()
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
                
                { "HttpServices:Clients:1:Service", "customers" },
                { "HttpServices:Clients:1:TimeoutInSeconds", "10" },
                
                { "HttpServices:HttpPolicies:Default:NumberOfRetries", "1" },
                { "HttpServices:HttpPolicies:Default:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:HttpPolicies:Default:RetryProgressionBaseValueInSeconds", "2" },
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dicConfig)
                .Build();

            return configuration;
        }

        private static IConfiguration CreateConfigurationWithoutServices()
        {
            var dicConfig = new Dictionary<string, string>
            {
                { "Mode", "MemoryConfigProvider" },
                
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