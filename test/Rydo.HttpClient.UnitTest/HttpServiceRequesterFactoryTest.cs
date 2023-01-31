namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using Configurations;
    using Converters;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Xunit;

    internal class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class HttpServiceRequesterFactoryTest : IDisposable
    {
        private ILoggerFactory _logger;
        private IHttpClientFactory _httpClientFactory;
        private IEndpointEntriesContext _endpointEntriesContext;

        //setup
        public HttpServiceRequesterFactoryTest()
        {
            _logger = Substitute.For<ILoggerFactory>();
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _endpointEntriesContext = Substitute.For<IEndpointEntriesContext>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Throw_Exception_When_ServiceName_IsNull_Or_Empty(string endpointName)
        {
            //arrange
            var sut = CreateSut();

            //assert
            Assert.Throws<ArgumentNullException>(() => sut.CreateRequestFor(endpointName));
        }

        [Fact]
        public void Should_Throw_Exception_When_EndpointName_Not_Found_In_Context()
        {
            //arrange

            _endpointEntriesContext = new EndpointEntriesContext();
            var sut = CreateSut();

            //act - assert
            Assert.Throws<InvalidOperationException>(() => sut.CreateRequestFor("calendar-data"));
        }

        [Fact]
        public void Should_Create_Valid_HttpRequester_With_Valid_EndpointName()
        {
            //arrange
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = ValidClientEntries(productsServiceName);

            _endpointEntriesContext = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });

            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
            var sut = new HttpServiceRequesterFactory(_logger, _httpClientFactory, _endpointEntriesContext);

            //act
            var requester = sut.CreateRequestFor("calendar-data");

            //assert
            requester.Should().NotBeNull();
        }
        
        [Fact]
        public void Should_Create_Valid_HttpRequester_With_Valid_EndpointName_And_Valid_JsonConverter()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = ValidClientEntries(productsServiceName);

            _endpointEntriesContext = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });

            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
            var sut = new HttpServiceRequesterFactory(_logger, _httpClientFactory, _endpointEntriesContext);

            //act
            var requester = sut.SetCustomConvert(new ListToListKeyValuePairJsonConverter()).CreateRequestFor("calendar-data");

            //assert
            requester.Should().NotBeNull();
        }

        [Fact]
        public void Should_Create_Valid_HttpRequester_Reusing_Serializer_Cache()
        {
            //arrange
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = ValidClientEntries(productsServiceName);

            _endpointEntriesContext = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });

            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient());
            var sut = new HttpServiceRequesterFactory(_logger, _httpClientFactory, _endpointEntriesContext);

            //act
            var requester = sut.CreateRequestFor("calendar-data");

            //assert
            requester.Should().NotBeNull();

            requester = sut.CreateRequestFor("calendar-data");
            requester.Should().NotBeNull();
        }

        private IHttpServiceRequesterFactory CreateSut() =>
            new HttpServiceRequesterFactory(_logger, _httpClientFactory, _endpointEntriesContext);

        private List<ClientEntry> ValidClientEntries(string productsServiceName)
        {

            return new List<ClientEntry>
            {
                new()
                {
                    Service = productsServiceName,
                    Endpoints = new[]
                    {
                        new EndpointEntry
                        {
                            Name = "calendar-data",
                            Path = "/api/product/{id}",
                        }
                    }
                }
            };
        }

        //teardown
        public void Dispose()
        {
            _logger = null;
            _httpClientFactory = null;
            _endpointEntriesContext = null;
        }
    }
}