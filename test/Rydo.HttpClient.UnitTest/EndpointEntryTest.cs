namespace Rydo.HttpClient.UnitTest
{
    using Configurations;
    using FluentAssertions;
    using Xunit;

    public class EndpointEntryTest
    {
        [Fact]
        public void Should_Return_HandlerLifetime_From_Settings_Without_Services_Binding()
        {
            const int handlerLifetimeInMinutes = 10;

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}",
                HandlerLifetimeInMinutes = handlerLifetimeInMinutes
            };

            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutes);
        }

        [Fact]
        public void Should_Return_HandlerLifetime_HttpClient_Without_Services_Binding_1()
        {
            const int handlerLifetimeInMinutesDefault = 2;

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}"
            };

            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutesDefault);
        }

        [Fact]
        public void Should_Return_HandlerLifetime_HttpClient_Default()
        {
            const int handlerLifetimeInMinutesDefault = 2;
            
            var serviceEntry = new ServiceEntry
            {
                Name = "products",
                BaseAddress = "http://localhost:5010"
            };

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}"
            };

            endpointEntry.BindService(serviceEntry);
            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutesDefault);
        }
        
        [Fact]
        public void Should_Return_HandlerLifetime_From_ServiceEntry()
        {
            const int handlerLifetimeInMinutes = 5;
            
            var serviceEntry = new ServiceEntry
            {
                Name = "products",
                BaseAddress = "http://localhost:5010",
                HandlerLifetimeInMinutes = handlerLifetimeInMinutes
            };

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}"
            };

            endpointEntry.BindService(serviceEntry);
            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutes);
        }
        
        [Fact]
        public void Should_Return_HandlerLifetime_From_EndpointEntry()
        {
            const int handlerLifetimeInMinutesFromEndpointEntry = 10;
            
            var serviceEntry = new ServiceEntry
            {
                Name = "products",
                BaseAddress = "http://localhost:5010",
                HandlerLifetimeInMinutes = 5
            };

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}",
                HandlerLifetimeInMinutes = handlerLifetimeInMinutesFromEndpointEntry
            };

            endpointEntry.BindService(serviceEntry);
            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutesFromEndpointEntry);
        }
        
        [Fact]
        public void Should_Return_HandlerLifetime_From_EndpointEntry_Without_ServiceEntry()
        {
            const int handlerLifetimeInMinutesFromEndpointEntry = 10;

            var serviceEntry = new ServiceEntry
            {
                Name = "products",
                BaseAddress = "http://localhost:5010"
            };

            var endpointEntry = new EndpointEntry
            {
                Name = "product",
                Path = "/api/product/{id}",
                HandlerLifetimeInMinutes = handlerLifetimeInMinutesFromEndpointEntry
            };

            endpointEntry.BindService(serviceEntry);
            endpointEntry.HandlerLifetimeInMinutes.Should().Be(handlerLifetimeInMinutesFromEndpointEntry);
        }
    }
}