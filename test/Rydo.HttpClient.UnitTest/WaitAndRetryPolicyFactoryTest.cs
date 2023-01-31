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
    using Polly.NoOp;
    using Polly.Retry;
    using Polly.Timeout;
    using Xunit;

    public class WaitAndRetryPolicyFactoryTest
    {
        private readonly Dictionary<string, object> _context;
        private readonly Mock<ILogger<HttpServiceRequester>> _mockLogger;

        public WaitAndRetryPolicyFactoryTest()
        {
            _mockLogger = new Mock<ILogger<HttpServiceRequester>>();
            _mockLogger.Setup(s => s.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _context = new Dictionary<string, object>
            {
                { Constants.PolicyContextLogger, _mockLogger.Object },
                { Constants.XCorrelationId, Guid.NewGuid().ToString() },
                { Constants.EndpointEntryName, Constants.EndpointEntryName }
            };
        }

        [Theory]
        [InlineData(2, 1, 0, 3)]
        [InlineData(2, 1, 1, 3)]
        [InlineData(2, 1, 2, 3)]
        [InlineData(2, 1, 3, 3)]
        [InlineData(3, 4, 0, 7)]
        [InlineData(3, 4, 1, 7)]
        [InlineData(3, 4, 2, 7)]
        [InlineData(3, 4, 3, 7)]
        public void WaitFunction_NoneProgression_Retry(
            int retryProgression,
            int retryWait,
            int retryAttempt,
            int expectedResult)
        {
            var endpointEntry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    RetryProgressionBaseValueInSeconds = retryProgression,
                    FirstRetryWaitTimeInSeconds = retryWait,
                    ProgressionType = TimeProgressionType.None
                }
            };

            var time = WaitAndRetryPolicyFactory.WaitStrategyRetries(retryAttempt, endpointEntry);

            Assert.Equal(TimeSpan.FromSeconds(expectedResult), time);
        }

        [Theory]
        [InlineData(2, 1, 0, 1)]
        [InlineData(2, 1, 1, 3)]
        [InlineData(2, 1, 2, 5)]
        [InlineData(2, 1, 3, 7)]
        [InlineData(3, 4, 0, 4)]
        [InlineData(3, 4, 1, 7)]
        [InlineData(3, 4, 2, 10)]
        [InlineData(3, 4, 3, 13)]
        public void WaitFunction_ArithmeticProgression_Retry(
            int retryProgression,
            int retryWait,
            int retryAttempt,
            int expectedResult)
        {
            var endpointEntry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    RetryProgressionBaseValueInSeconds = retryProgression,
                    FirstRetryWaitTimeInSeconds = retryWait,
                    ProgressionType = TimeProgressionType.Arithmetic
                }
            };

            var time = WaitAndRetryPolicyFactory.WaitStrategyRetries(retryAttempt, endpointEntry);

            Assert.Equal(TimeSpan.FromSeconds(expectedResult), time);
        }

        [Theory]
        [InlineData(2, 1, 0, 2)]
        [InlineData(3, 4, 0, 5)]
        [InlineData(2, 1, 1, 3)]
        [InlineData(2, 1, 2, 5)]
        [InlineData(2, 1, 3, 9)]
        [InlineData(3, 4, 1, 7)]
        [InlineData(3, 4, 2, 13)]
        [InlineData(3, 4, 3, 31)]
        public void WaitFunction_GeometricProgression_Retry(
            int retryProgression,
            int retryWait,
            int retryAttempt,
            int expectedResult)
        {
            var endpointEntry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    FirstRetryWaitTimeInSeconds = retryWait,
                    RetryProgressionBaseValueInSeconds = retryProgression,
                    ProgressionType = TimeProgressionType.Geometric
                }
            };

            var time = WaitAndRetryPolicyFactory.WaitStrategyRetries(retryAttempt, endpointEntry);
            Assert.Equal(TimeSpan.FromSeconds(expectedResult), time);
        }

        [Fact]
        public void WaitFunction_NullEndpoint_ThrowException()
        {
            Func<TimeSpan> action = () => WaitAndRetryPolicyFactory.WaitStrategyRetries(1, null);
            var exception = Assert.Throws<ArgumentNullException>(() => action());
            Assert.Equal("Value cannot be null. (Parameter 'endpointEntry')", exception.Message);
        }

        [Fact]
        public void GetWaitAndRetryPolicy_NullEndpoint_ThrowException()
        {
            EndpointEntry entry = null;
            Action action = () => entry.GetWaitAndRetryPolicy();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'endpointEntry')", exception.Message);
        }

        [Fact]
        public void GetWaitAndRetryPolicy_NoRetry_NoOpAsync()
        {
            var endpointEntry = new EndpointEntry
            {
                Retry = null
            };

            var policy = endpointEntry.GetWaitAndRetryPolicy();

            Assert.IsType<AsyncNoOpPolicy<HttpResponseMessage>>(policy);
        }

        [Fact]
        public void GetWaitAndRetryPolicy_Retry()
        {
            var endpointEntry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    FirstRetryWaitTimeInSeconds = 1,
                    RetryProgressionBaseValueInSeconds = 1,
                    ProgressionType = TimeProgressionType.None
                }
            };

            var policy = endpointEntry.GetWaitAndRetryPolicy();

            Assert.IsType<AsyncRetryPolicy<HttpResponseMessage>>(policy);
        }

#pragma warning disable CS0169, CS1998

        [Fact]
        public async Task CreateAsyncPolicy_TestExceptionMessage()
        {
            string message = "The request timed out";

            Func<int, HttpResponseMessage> funcCall = (callCount) =>
            {
                var response = new HttpResponseMessage();
                if (callCount <= 1)
                {
                    response.StatusCode = System.Net.HttpStatusCode.RequestTimeout;
                    throw new HttpRequestException(message);
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                return response;
            };

            var entry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = 3
                }
            };

            int numberOfCalls = 0;

            var policy = WaitAndRetryPolicyFactory.GetWaitAndRetryPolicy(entry);

            var resp = await policy.ExecuteAsync(async (ctx) => FakeCall(ref numberOfCalls, funcCall), _context);

            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);

            _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.Is<EventId>(id => id.Name == "request-policy-retried"),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once());
        }

        [Fact]
        public async Task CreateAsyncPolicy_TestTimeoutRejectedException()
        {
            string message = "The request timed out";

            Func<int, HttpResponseMessage> funcCall = (callCount) =>
            {
                var response = new HttpResponseMessage();
                if (callCount <= 1)
                {
                    response.StatusCode = System.Net.HttpStatusCode.Ambiguous;
                    throw new TimeoutRejectedException(message);
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                return response;
            };

            var entry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = 3
                }
            };

            int numberOfCalls = 0;

            var policy = WaitAndRetryPolicyFactory.GetWaitAndRetryPolicy(entry);

            var resp = await policy.ExecuteAsync(async (ctx) => FakeCall(ref numberOfCalls, funcCall), _context);

            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);

             _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.Is<EventId>(id => id.Name == "request-policy-retried"),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once());
        }

        [Fact]
        public async Task CreateAsyncPolicy_TestTimeoutRejectedException_NoLogger()
        {
            const string message = "The request timed out";

            Func<int, HttpResponseMessage> funcCall = (callCount) =>
            {
                var response = new HttpResponseMessage();
                if (callCount <= 1)
                {
                    response.StatusCode = System.Net.HttpStatusCode.Ambiguous;
                    throw new TimeoutRejectedException(message);
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                return response;
            };

            var entry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = 3
                }
            };

            int numberOfCalls = 0;

            var policy = WaitAndRetryPolicyFactory.GetWaitAndRetryPolicy(entry);

            var resp = await policy.ExecuteAsync(async () => FakeCall(ref numberOfCalls, funcCall));

            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);

            _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never());

            _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never());
        }

        [Fact]
        public async Task CreateAsyncPolicy_NoException()
        {
            const string message = "The request timed out";

            Func<int, HttpResponseMessage> funcCall = (callCount) =>
            {
                var response = new HttpResponseMessage();
                if (callCount <= 1)
                {
                    response.StatusCode = System.Net.HttpStatusCode.RequestTimeout;
                    return response;
                }

                response.StatusCode = System.Net.HttpStatusCode.OK;
                return response;
            };

            var entry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = 3
                }
            };

            int numberOfCalls = 0;

            var policy = entry.GetWaitAndRetryPolicy();

            var resp = await policy.ExecuteAsync(async (ctx) => FakeCall(ref numberOfCalls, funcCall), _context);

            Assert.NotNull(resp);
            Assert.Equal(System.Net.HttpStatusCode.OK, resp.StatusCode);

            _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == message),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never());

            _mockLogger.Verify(mock => mock.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once());
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