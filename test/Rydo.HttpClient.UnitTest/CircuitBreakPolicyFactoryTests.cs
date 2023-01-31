namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Configurations;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Policies;
    using Polly.CircuitBreaker;
    using Polly.NoOp;
    using Polly.Timeout;
    using Xunit;

    public class CircuitBreakPolicyFactoryTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Dictionary<string, object> _context;
        private readonly Mock<IServiceProvider> _serviveProvider;

        public CircuitBreakPolicyFactoryTests()
        {
            _mockLogger = new Mock<ILogger>();
            _serviveProvider = new Mock<IServiceProvider>();

            _mockLogger.Setup(s => s.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _context = new Dictionary<string, object>
            {
                { Constants.PolicyContextLogger, _mockLogger.Object }
            };
        }

        [Fact]
        public void GetCircuitBreakPolicy_NullEndpoint_ThrowException()
        {
            EndpointEntry entry = null;
            Action action = () => entry.GetCircuitBreakPolicy();
            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'endpointEntry')", exception.Message);
        }

        [Fact]
        public void GetCircuitBreakPolicy_NoRetry_NoOpAsync()
        {
            var endpointEntry = new EndpointEntry
            {
                CircuitBreak = null
            };

            var policy = endpointEntry.GetCircuitBreakPolicy();

            Assert.IsType<AsyncNoOpPolicy<HttpResponseMessage>>(policy);
        }

        [Fact]
        public void GetCircuitBreakPolicy_TestBinding()
        {
            var endpointEntry = new EndpointEntry
            {
                CircuitBreak = new CircuitBreakPolicyEntry
                {
                    DurationOfBreakInSeconds = 2,
                    EventsTimesBeforeBreaking = 3
                }
            };

            var policy = endpointEntry.GetCircuitBreakPolicy();

            Assert.IsType<AsyncCircuitBreakerPolicy<HttpResponseMessage>>(policy);
        }

#pragma warning disable CS0169, CS1998

        [Fact]
        public async Task GetCircuitBreakPolicy_TestExceptionMessage()
        {
            string message = "The request timed out";

            Func<int, HttpResponseMessage> funcCall = (callCount) =>
            {
                var response = new HttpResponseMessage();
                if (callCount <= -1)
                {
                    response.StatusCode = System.Net.HttpStatusCode.Ambiguous;
                    throw new TimeoutRejectedException(message);
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                return response;
            };

            var entry = new EndpointEntry
            {
                CircuitBreak = new CircuitBreakPolicyEntry
                {
                    DurationOfBreakInSeconds = 2,
                    EventsTimesBeforeBreaking = 3
                }
            };

            int numberOfCalls = 0;

            var policy = CircuitBreakPolicyFactory.GetCircuitBreakPolicy(entry);

            var resp = await policy.ExecuteAsync(async (ctx) => FakeCall(ref numberOfCalls, funcCall), _context);

            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);

            /*
                _mockLogger.Verify(mock => mock.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once());
                _mockLogger.Verify(mock => mock.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ), Times.Once());
                */
        }

#pragma warning restore CS0169, CS1998

        private static HttpResponseMessage FakeCall(ref int callCount, Func<int, HttpResponseMessage> action)
        {
            callCount++;
            var resp = action(callCount);
            return resp;
        }
    }
}