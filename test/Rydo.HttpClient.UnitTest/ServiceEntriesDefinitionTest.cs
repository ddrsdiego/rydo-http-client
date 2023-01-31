namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configurations;
    using Exceptions;
    using FluentAssertions;
    using Xunit;

    public class ServiceEntriesDefinitionTest
    {
        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull()
        {
            List<ServiceEntry> serviceEntries = null;

            Assert.Throws<ArgumentNullException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }

        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull_Empty()
        {
            var serviceEntries = new List<ServiceEntry>();
            Assert.Throws<ArgumentNullException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }

        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull_Empty_1()
        {
            var serviceEntries = new List<ServiceEntry>();
            serviceEntries.Add(new ServiceEntry());

            Assert.Throws<ArgumentNullException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }

        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull_Empty_2()
        {
            var serviceEntries = new List<ServiceEntry>();
            serviceEntries.Add(new ServiceEntry { Name = "calendar" });

            Assert.Throws<ArgumentNullException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }

        //ServiceEntryAlreadyExistsException
        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull_Empty_3()
        {
            var serviceEntries = new List<ServiceEntry>();
            serviceEntries.Add(new ServiceEntry { Name = "calendar", RepositoryName = "calendar", BaseAddress = "http://localhost/api-test" });
            serviceEntries.Add(new ServiceEntry { Name = "calendar", RepositoryName = "calendar", BaseAddress = "http://localhost/api-test" });

            Assert.Throws<ServiceEntryAlreadyExistsException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }

        [Fact]
        public void Should_Throw_Exception_When_ServiceEntries_List_IsNull_Empty_4()
        {
            var serviceEntries = new List<ServiceEntry>();
            serviceEntries.Add(new ServiceEntry { Name = "calendar", BaseAddress = "http://localhost/api-test" });
            serviceEntries.Add(null);

            Assert.Throws<ArgumentNullException>(() =>
                ServiceEntryEx.TryGetServiceEntries(x => x.AddNewServiceEntry(serviceEntries)));
        }
        
        [Fact]
        public void Test()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntriesContext =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            serviceEntriesContext.Entries.Should().NotBeEmpty();
            var serviceEntryExpected = serviceEntriesContext.Entries.First(x => x.Key == productsServiceName);
            serviceEntryExpected.Should().NotBeNull();
        }
    }
}