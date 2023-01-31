namespace Rydo.HttpClient.UnitTest
{
    using Serialization;
    using Xunit;

    public class DefaultSerializerTests
    {
        private readonly DefaultSerializer _serializer;
        private const string JsonTest = "{\"Id\":1,\"Name\":\"Test\"}";
        
        public DefaultSerializerTests()
        {
            _serializer = new DefaultSerializer();
        }

        [Fact]
        public void DefaultSerializer_TestSerialization()
        {
            var test = new TestSerializer
            {
                Id = 1,
                Name = "Test"
            };

            var resp = _serializer.Serialize<TestSerializer>(test);
            Assert.NotNull(resp);
            Assert.Equal(JsonTest, resp);
        }

        [Fact]
        public void DefaultSerializer_TestDeserialization()
        {
            var test = new TestSerializer
            {
                Id = 1,
                Name = "Test"
            };

            var resp = _serializer.Deserialize<TestSerializer>(JsonTest);
            Assert.NotNull(resp);
            Assert.Equal(test.Id, resp.Id);
            Assert.Equal(test.Name, resp.Name);
        }

    }
}