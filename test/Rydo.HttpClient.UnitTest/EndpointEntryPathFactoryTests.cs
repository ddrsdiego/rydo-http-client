namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Configurations;
    using FluentAssertions;
    using Xunit;

    public class EndpointEntryPathFactoryTests
    {
        [Fact]
        public void GetEndpointPath_NullEntryPointName_ShouldThrowException()
        {
            var entry = new EndpointEntry();
            Action action = () => entry.GetEndpointPath();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'Name')", exception.Message);
        }

        [Fact]
        public void GetEndpointPath_NullPath_ShouldThrowException()
        {
            var entry = new EndpointEntry
            {
                Name = "Name"
            };

            Action action = () => entry.GetEndpointPath();

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'endpointString')", exception.Message);
        }

        [Fact]
        public void GetEndpointPath_EntryPointAlreadyDefined()
        {
            string entryPointName = "EntryPoint";
            Uri uri = new Uri("http://localhost");
            var dic = new Dictionary<string, Uri> {{entryPointName, uri}};
            EndpointEntryPathFactory.Uris = dic.ToImmutableDictionary();

            var entry = new EndpointEntry
            {
                Name = entryPointName
            };

            var newUri = entry.GetEndpointPath();
            Assert.Equal(uri, newUri);

        }

        [Fact]
        public void GetEndpointPath_BaseWithoutSlash()
        {
            const string entryPointName = "EntryPoint";
            const string baseAddress = "http://localhost";
            const string path = "id/resource";
            
            var serviceEntry = new ServiceEntry
            {
                BaseAddress = baseAddress
            };

            var entry = new EndpointEntry
            {
                Name = entryPointName,
                Path = path
            };

            entry.BindService(serviceEntry);

            var newUri = entry.GetEndpointPath();

            $"{baseAddress}/{path}".Should().Be(newUri.ToString());
            // Assert.Equal($"{baseAddress}/{path}", newUri.ToString());
        }

        [Fact]
        public void GetEndpointPath_PathWithSlash()
        {
            const string entryPointName = "EntryPoint";
            const string baseAddress = "http://localhost/";
            const string path = "id/resource";
            
            var serviceEntry = new ServiceEntry
            {
                BaseAddress = baseAddress
            };

            var entry = new EndpointEntry
            {
                Name = entryPointName,
                Path = "/" + path
            };

            entry.BindService(serviceEntry);

            var newUri = entry.GetEndpointPath();
            
            Assert.Equal($"{baseAddress}{path}", newUri.ToString());
        }

        [Fact]
        public void GetEndpointPath_TestBinding()
        {
            const string entryPointName = "EntryPoint";
            const string baseAddress = "http://localhost/";
            const string path = "id/resource";

            var serviceEntry = new ServiceEntry
            {
                BaseAddress = baseAddress
            };

            var entry = new EndpointEntry
            {
                Name = entryPointName,
                Path = path
            };

            entry.BindService(serviceEntry);

            var newUri = entry.GetEndpointPath();
            
            Assert.Equal($"{baseAddress}{path}", newUri.ToString());
        }
    }
}