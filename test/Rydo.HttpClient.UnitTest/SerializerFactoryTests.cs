namespace Rydo.HttpClient.UnitTest
{
    using System;
    using FluentAssertions;
    using Serialization;
    using Xunit;

    public class SerializerFactoryTests
    {
        [Fact]
        public void SerializerFactory_DefaultSerializer()
        {
            var serializer = SerializerFactory.Create(null);
            Assert.IsType<DefaultSerializer>(serializer);
        }

        [Fact]
        public void SerializerFactory_DefaultSerializer_EmptyName()
        {
            var serializer = SerializerFactory.Create(string.Empty);
            Assert.IsType<DefaultSerializer>(serializer);
        }

        [Fact]
        public void SerializerFactory_SnakeSerializer()
        {
            var serializer = SerializerFactory.Create("Snake");
            Assert.IsType<SnakeCaseSerializer>(serializer);
        }

        [Fact]
        public void SerializerFactory_WrongSerializer_ThrowException()
        {
            const string serializerName = "Snake_Test";
            Action action = () => SerializerFactory.Create(serializerName);
            var exception = Assert.Throws<ArgumentException>(action);
            exception.Message.Should().Be(SerializerFactory.CreateExceptionMessage(serializerName));
        }
    }
}