namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using Configurations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class EndpointEntriesContextExTests : IClassFixture<ConfigurationFixture>
    {
        private readonly ConfigurationFixture _fixture;
        private readonly ServiceCollection _services;
        public EndpointEntriesContextExTests(ConfigurationFixture fixture)
        {
            _fixture = fixture;
            _services = new ServiceCollection();

        }

        [Fact]
        public void TryGetEndpointEntriesDefinition_NoConfig_ShouldThrowException()
        {
            var configuration = new ConfigurationBuilder().Build();
            Action action = () => _services.TryGetEndpointEntriesDefinition(configuration, null);
            var exception = Assert.Throws<InvalidOperationException>(() => action());
            Assert.Equal(EndpointEntriesContextEx.NoDefinedClientException, exception.Message);
        }

        [Fact]
        public void TryGetEndpointEntriesDefinition_NoClientNode_ShouldThrowException()
        {
            var dicConfig = new Dictionary<string, string>
            {
                { "Mode", "MemoryConfigProvider" },
                { "HttpServices:Services:0:Name", "products" },
                { "HttpServices:Services:0:BaseAddress", "http://localhost:5010" },
                { "HttpServices:Services:1:Name", "customers" },
                { "HttpServices:Services:1:BaseAddress", "http://localhost:5020" },
                { "HttpServices:Clients", "" },
                { "HttpServices:HttpPolicies:Default:NumberOfRetries", "1" },
                { "HttpServices:HttpPolicies:Default:FirstRetryWaitTimeInSeconds", "1" },
                { "HttpServices:HttpPolicies:Default:RetryProgressionBaseValueInSeconds", "2" },
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dicConfig)
                .Build();

            Action action = () => _services.TryGetEndpointEntriesDefinition(configuration, null);
            var exception = Assert.Throws<InvalidOperationException>(() => action());
            Assert.Equal(EndpointEntriesContextEx.NoDefinedClientException, exception.Message);
        }

        [Fact]
        public void TryGetClientEntries_EmptyAction()
        {
            Action<EndpointEntriesContext> action = null;
            Action testAction = () => EndpointEntriesContextEx.TryGetClientEntries(action);
            var exception = Assert.Throws<ArgumentNullException>(() => testAction());
        }

        [Fact]
        public void TryGetClientEntries_ExecuteAction()
        {
            EndpointEntriesContext contextReturn = null;
            Action<EndpointEntriesContext> action = (endpoint) => 
            {
                Assert.NotNull(endpoint);
                Assert.IsType<EndpointEntriesContext>(endpoint);
                contextReturn = endpoint;
            };

            var ret = EndpointEntriesContextEx.TryGetClientEntries(action);
            Assert.Equal(ret, contextReturn);
            
        }
    }
}