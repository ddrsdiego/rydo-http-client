namespace Rydo.HttpClient.UnitTest
{
    using System;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Polly;
    using Xunit;

    internal class TestSerializer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class ContextPollyExTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Context _context;

        public ContextPollyExTests()
        {
            _mockLogger = new Mock<ILogger>();
            _context = new Context();
            _context.Add(Constants.PolicyContextLogger, _mockLogger.Object);
        }

        [Fact]
        public void GetLogger_ThrowExceptionWhenNull()
        {
            Context context = null;
            Action action = () => context.GetLogger();

            var exception = Assert.Throws<ArgumentNullException>(() => action());
            Assert.Equal("Value cannot be null. (Parameter 'context')", exception.Message);
        }

        [Fact]
        public void GetLogger_ReturnNullWhenNotFound()
        {
            var context = new Context();
            var logger = context.GetLogger();
            Assert.Null(logger);
        }

        [Fact]
        public void GetLogger_ReturnWhenFound()
        {
            var logger = _context.GetLogger();
            Assert.NotNull(logger);
            Assert.Equal(_mockLogger.Object, logger);
        }
    }
}