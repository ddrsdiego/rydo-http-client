namespace Rydo.HttpClient.UnitTest
{
    using System;
    using Formatters;
    using Xunit;

    internal class TestDtoSnake
    {
        public int IdCustomer { get; set; }
        public string FirstName { get; set; }
    }
    
    public class UriFormatterTests
    {
        [Fact]
        public void UriFormatter_Format_EmptyTemplate()
        {
            var template = "/1/resource/test";
            var formated = UriFormatter.Format(template, 1, 2);
            Assert.Equal(template, formated);
        }

        [Fact]
        public void UriFormatter_Format_InvalidNumberOfParameters_ThrowException()
        {
            const string template = "/{id}/resource/test={name}";
            string Action() => UriFormatter.Format(template, 1);
            var exception = Assert.Throws<InvalidOperationException>((Func<string>) Action);
            Assert.Equal(UriFormatter.InvalidTemplateException, exception.Message);
        }

        [Fact]
        public void UriFormatter_Format_TestBinding()
        {
            const string template = "/{id}/resource/test={name}";
            var formatted = UriFormatter.Format(template, 1, "first");
            Assert.Equal("/1/resource/test=first", formatted);
        }
    }
}