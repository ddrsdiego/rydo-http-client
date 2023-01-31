namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Configurations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class ServiceEntryExTests
    {
        [Fact]
        public void TryGetServiceEntries_NoService_ThrowException()
        {
            var services = new ServiceCollection();
            Action action = () => services.TryGetServiceEntries(CreateConfigurationWithoutServices());
            var exception = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(ServiceEntryEx.NoServiceConfigurationException, exception.Message);
        }

        [Fact]
        public void TryGetServiceEntries_TestBinding()
        {
            var services = new ServiceCollection();
            var entries = services.TryGetServiceEntries(CreateValidConfiguration());
            Assert.Equal(2, entries.Entries.Count);
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