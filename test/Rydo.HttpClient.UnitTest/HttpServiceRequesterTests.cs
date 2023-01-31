namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configurations;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Moq.Protected;
    using Serialization;
    using Xunit;

    public class HttpServiceRequesterTests
    {
        private readonly HttpClient _client;
        private readonly Mock<HttpMessageHandler> _handler;
        private readonly Mock<ISerializer> _serializer;
        private readonly Mock<ILogger<HttpServiceRequester>> _logger;
        private readonly EndpointEntry _entry;
        private readonly HttpServiceRequester _requester;
        private readonly string _baseAddress = "http://localhost";
        private const string JSON_TEST = "{\"Id\":1,\"Name\":\"Test\"}";

        public HttpServiceRequesterTests()
        {
            var service = new ServiceEntry
            {
                BaseAddress = _baseAddress
            };

            _entry = new EndpointEntry
            {
                Name = "Test",
                Path = "{id}/resource?page={page}",
            };
            _entry.BindService(service);

            _handler = new Mock<HttpMessageHandler>();
            _client = new HttpClient(_handler.Object);
            _serializer = new Mock<ISerializer>();
            _logger = new Mock<ILogger<HttpServiceRequester>>();

            _requester = new HttpServiceRequester(_client, _entry, _serializer.Object, _logger.Object);
        }

        [Fact]
        public void HttpServiceRequester_NullClient()
        {
            HttpServiceRequester requester = null;
            Action action = () => requester = new HttpServiceRequester(null, _entry, _serializer.Object, _logger.Object);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'httpClient')", exception.Message);
            Assert.Null(requester);
        }

        [Fact]
        public void HttpServiceRequester_NullEntrypoint()
        {
            HttpServiceRequester requester = null;
            Action action = () => requester = new HttpServiceRequester(_client, null, _serializer.Object, _logger.Object);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'endpointEntry')", exception.Message);
            Assert.Null(requester);
        }

        [Fact]
        public void HttpServiceRequester_NullSerializer()
        {
            HttpServiceRequester requester = null;
            Action action = () => requester = new HttpServiceRequester(_client, _entry, null, _logger.Object);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'serializer')", exception.Message);
            Assert.Null(requester);
        }

        [Fact]
        public void HttpServiceRequester_WithParameters()
        {
            int id = 1;
            string page = "last";
            _requester.WithParameters(id, page);

            Assert.Equal($"{_baseAddress}/1/resource?page=last", _requester.Uri.ToString());
        }

        [Fact]
        public void HttpServiceRequester_WithHeader_NullName()
        {
            string value = "x-unit-text";

            Action action = () => _requester.WithHeader(null, value);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'name')", exception.Message);
            Assert.Empty(_requester.Headers);
        }

        [Fact]
        public void HttpServiceRequester_WithHeader_NullValue()
        {
            const string name = "unit-test";
            const string message = "Value cannot be null. (Parameter 'value')";

            string value = null;
            Action action = () => _requester.WithHeader(name, value);

            var exception = Assert.Throws<ArgumentNullException>(action);

            _requester.Headers.Should().BeEmpty();
            message.Should().Be(exception.Message);
        }

        [Fact]
        public void HttpServiceRequester_WithHeader()
        {
            const string name = "unit-test";
            const string value = "x-unit-text";

            _requester.WithHeader(name, value);

            _requester.Headers.Count().Should().Be(1);
            _requester.Headers.GetValues(name).First().Should().Be(value);
        }

        [Fact]
        public void HttpServiceRequester_WithToken_NullToken()
        {
            const string message = "Value cannot be null. (Parameter 'token')";

            string token = null;
            Action action = () => _requester.WithBearerToken(token);

            var exception = Assert.Throws<ArgumentNullException>(action);
            
            _requester.Headers.Should().BeEmpty();
            exception.Message.Should().Be(message);
        }

        [Fact]
        public void HttpServiceRequester_WithToken()
        {
            const string token = "ABC123456";

            _requester.WithBearerToken(token);
            Assert.Single(_requester.Headers);
            Assert.Equal(token, _requester.Headers.Authorization.Parameter);
            Assert.Equal("Bearer", _requester.Headers.Authorization.Scheme);
        }

        [Fact]
        public async Task HttpServiceRequester_GetAsync()
        {
            // GIVEN

            var response = new HttpResponseMessage();
            response.Content = new StringContent(JSON_TEST);

            var testDto = new TestDto { Id = 1, Name = "Test" };

            _serializer.Setup(s => s.Deserialize<TestDto>(It.IsAny<string>()))
                .Returns(testDto);
            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            const int id = 1;
            const string page = "last";

            var resp = await _requester.WithParameters(id, page)
                .GetAsync<TestDto>();

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.NotNull(resp.Result);
            Assert.IsType<TestDto>(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task HttpServiceRequester_GetAsync_NotFound()
        {
            // GIVEN
            string message = "Resource Not Found";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            response.Content = new StringContent(message);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .GetAsync<TestDto>();

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.Null(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, resp.StatusCode);
            Assert.Equal(message, await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task HttpServiceRequester_PostAsync()
        {
            // GIVEN

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Created);
            response.Content = new StringContent(JSON_TEST);

            var testDtoResp = new TestDto { Id = 1, Name = "Test1" };
            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Deserialize<TestDto>(It.IsAny<string>()))
                .Returns(testDtoResp);

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PostAsync<TestDto, TestDto>(testDtoReq);

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.NotNull(resp.Result);
            Assert.IsType<TestDto>(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact]
        public async Task HttpServiceRequester_PostAsync_InternalServerError()
        {
            // GIVEN
            string message = "Internal server error";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            response.Content = new StringContent(message);

            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PostAsync<TestDto>(testDtoReq);

            // THEN
            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, resp.StatusCode);
            Assert.Equal(message, await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task HttpServiceRequester_PutAsync()
        {
            // GIVEN

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Created);
            response.Content = new StringContent(JSON_TEST);

            var testDtoResp = new TestDto { Id = 1, Name = "Test1" };
            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Deserialize<TestDto>(It.IsAny<string>()))
                .Returns(testDtoResp);

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PutAsync<TestDto, TestDto>(testDtoReq);

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.NotNull(resp.Result);
            Assert.IsType<TestDto>(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact]
        public async Task HttpServiceRequester_PutAsync_BadRequest()
        {
            // GIVEN
            string message = "Bad Request";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            response.Content = new StringContent(message);

            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PutAsync<TestDto>(testDtoReq);

            // THEN
            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal(message, await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task HttpServiceRequester_DeleteAsync()
        {
            // GIVEN

            var response = new HttpResponseMessage();
            response.Content = new StringContent(JSON_TEST);

            var testDto = new TestDto { Id = 1, Name = "Test" };

            _serializer.Setup(s => s.Deserialize<TestDto>(It.IsAny<string>()))
                .Returns(testDto);
            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .DeleteAsync<TestDto>();

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.NotNull(resp.Result);
            Assert.IsType<TestDto>(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);
        }

        [Fact]
        public async Task HttpServiceRequester_DeleteAsync_NotFound()
        {
            // GIVEN
            const string message = "Resource Not Found";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            response.Content = new StringContent(message);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            const int id = 1;
            const string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .DeleteAsync();

            // THEN
            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, resp.StatusCode);
            Assert.Equal(message, await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task HttpServiceRequester_PatchAsync()
        {
            // GIVEN

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.Created);
            response.Content = new StringContent(JSON_TEST);

            var testDtoResp = new TestDto { Id = 1, Name = "Test1" };
            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Deserialize<TestDto>(It.IsAny<string>()))
                .Returns(testDtoResp);

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            const int id = 1;
            const string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PatchAsync<TestDto, TestDto>(testDtoReq);

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.NotNull(resp.Result);
            Assert.IsType<TestDto>(resp.Result);
            Assert.Equal(System.Net.HttpStatusCode.Created, resp.StatusCode);
        }

        [Fact]
        public async Task HttpServiceRequester_PatchAsync_InternalServerError()
        {
            // GIVEN
            string message = "Internal server error";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
            response.Content = new StringContent(message);

            var testDtoReq = new TestDto { Id = 2, Name = "Test2" };

            _serializer.Setup(s => s.Serialize<TestDto>(It.IsAny<TestDto>()))
                .Returns(JSON_TEST);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .PatchAsync<TestDto, TestDto>(testDtoReq);

            // THEN
            Assert.NotEqual(default(HttpServiceResponse<TestDto>), resp);
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, resp.StatusCode);
            Assert.Equal(message, await resp.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task HttpServiceRequester_GetStringAsync()
        {
            // GIVEN
            string message = "String Message";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(message);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .GetStringAsync();

            // THEN
            Assert.NotNull(resp);
            Assert.Equal(message, resp);
        }

        [Fact]
        public async Task HttpServiceRequester_GetByteArrayAsync()
        {
            // GIVEN
            string message = "String Message";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new ByteArrayContent(Encoding.ASCII.GetBytes(message));

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            int id = 1;
            string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .GetByteArrayAsync();

            // THEN
            Assert.NotNull(resp);
            Assert.Equal(message, Encoding.ASCII.GetString(resp));
        }

        [Fact]
        public async Task HttpServiceRequester_GetStreamAsync()
        {
            // GIVEN
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);

            const string message = "String Message";

            streamWriter.Write(message);
            streamWriter.Flush();
            memoryStream.Position = 0;

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StreamContent(memoryStream);

            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // WHEN

            const int id = 1;
            const string page = "last";
            var resp = await _requester.WithParameters(id, page)
                .GetStreamAsync();

            // THEN
            Assert.NotNull(resp);
            using var reader = new StreamReader(resp);
            Assert.Equal(message, reader.ReadToEnd());
        }
    }
}