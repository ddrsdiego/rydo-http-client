namespace Rydo.HttpClient.UnitTest
{
    using Serialization;
    using Xunit;

    public class SnakeCaseNamingPolicyTests
    {
        private const string JSON_TEST = "{\"id_customer\":1,\"first_name\":\"Test\"}";

        [Theory]
        [InlineData("TestString","test_string")]
        [InlineData("meuNome","meu_nome")]
        [InlineData("underscore","underscore")]
        [InlineData("UmaStringGigantePraTestar","uma_string_gigante_pra_testar")]
        public void SnakeCaseNamingPolicy_Test(string name, string snakeName)
        {
            var snake = new SnakeCaseNamingPolicy();
            var snakeString = snake.ConvertName(name);
            Assert.Equal(snakeName, snakeString);
        }

        [Fact]
        public void SnakeCaseSerializer_Serialize()
        {
            var serializer = new SnakeCaseSerializer();
            var dto = new TestDtoSnake
            {
                IdCustomer = 1,
                FirstName = "Test"
            };

            var json = serializer.Serialize(dto);
            Assert.Equal(JSON_TEST, json);
        }

        [Fact]
        public void SnakeCaseSerializer_Deserialize()
        {
            var serializer = new SnakeCaseSerializer();
            var dto = new TestDtoSnake
            {
                IdCustomer = 1,
                FirstName = "Test"
            };

            var json = serializer.Deserialize<TestDtoSnake>(JSON_TEST);
            Assert.Equal(dto.FirstName, json.FirstName);
            Assert.Equal(dto.IdCustomer, json.IdCustomer);
        }
    }
}