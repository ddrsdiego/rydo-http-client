namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Net.Http;
    using Configurations;
    using Policies;
    using Xunit;

    public class HttpMessageHandlerFactoryTests
    {
        [Fact]
        public void GetHttpMessageHandler_NoEntry_ThrowException()
        {
            EndpointEntry entry = null;
            Action action = () => entry.GetHttpMessageHandler();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'endpointEntry')", exception.Message);
        }

        [Fact]
        public void GetHttpMessageHandler_EmptyPath_NoSslOption()
        {
            var entry = new EndpointEntry();
            var handler = entry.GetHttpMessageHandler();
            Assert.NotNull(handler);
            var socket = handler as StandardSocketsHttpHandler;
            Assert.Null(socket.SslOptions.ClientCertificates);
        }

        [Fact]
        public void GetHttpMessageHandler_WrongPath_NoSslOption()
        {
            var client = new ClientEntry();

            var entry = new EndpointEntry
            {
            };

            entry.BindClient(client);

            var handler = entry.GetHttpMessageHandler();
            Assert.NotNull(handler);
            var socket = handler as StandardSocketsHttpHandler;
            Assert.Null(socket.SslOptions.ClientCertificates);
        }
    }
}