namespace Rydo.HttpClient.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Configurations;
    using Exceptions;
    using Xunit;

    public class EndpointEntriesContextTests
    {
        [Fact]
        public void AddEndpointEntry_NullClientEntry_ShouldThrowException()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            Action action = () => EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, null, new HttpPoliciesEntry());
            });

            var exception = Assert.Throws<ArgumentNullException>(() => action());
            Assert.Equal("Value cannot be null. (Parameter 'clientEntries')", exception.Message);
        }

        [Fact]
        public void AddEndpointEntry_NullService_ShouldNotAdd()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = new List<ClientEntry>
            {
                new()
                {
                    Service = null,
                    Endpoints = new[]
                    {
                        new EndpointEntry
                        {
                            Name = "product",
                            Path = "/api/product/{id}",
                        },
                        new EndpointEntry
                        {
                            Name = "product-prices",
                            Path = "/api/product/{id}/prices"
                        }
                    }
                }
            };

            var resp = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });
            Assert.Empty(resp.Entries);
        }

        [Fact]
        public void AddEndpointEntry_ServiceNotConfigured_ShouldNotAdd()
        {
            const string productsServiceName = "products";
            const string productsServiceNameBogus = "wrong_products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = new List<ClientEntry>
            {
                new()
                {
                    Service = productsServiceNameBogus,
                    Endpoints = new[]
                    {
                        new EndpointEntry
                        {
                            Name = "product",
                            Path = "/api/product/{id}",
                        },
                        new EndpointEntry
                        {
                            Name = "product-prices",
                            Path = "/api/product/{id}/prices"
                        }
                    }
                }
            };

            var resp = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });
            Assert.Empty(resp.Entries);
        }

        [Fact]
        public void AddEndpointEntry_NullEndpoint_ShouldNotAdd()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = new List<ClientEntry>
            {
                new()
                {
                    Service = productsServiceName,
                    Endpoints = null
                }
            };

            var resp = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });
            Assert.Empty(resp.Entries);
        }

        [Fact]
        public void AddEndpointEntry_EmptyEndpoint_ShouldNotAdd()
        {
            const string productsServiceName = "products";

            var serviceEntry = new ServiceEntry { Name = productsServiceName, RepositoryName = productsServiceName, BaseAddress = "http://localhost:5020" };
            var serviceEntryDefinition =
                ServiceEntryEx.TryGetServiceEntries(
                    x => x.AddNewServiceEntry(new List<ServiceEntry> { serviceEntry }));

            var clientEntries = new List<ClientEntry>
            {
                new()
                {
                    Service = productsServiceName,
                    Endpoints = new EndpointEntry[0]
                }
            };

            var resp = EndpointEntriesContextEx.TryGetClientEntries(x =>
            {
                x.AddEndpointEntry(serviceEntryDefinition, clientEntries, new HttpPoliciesEntry());
            });
            Assert.Empty(resp.Entries);
        }

        [Fact]
        public void AddEndpointEntry_NullEndpointName_ShouldThrowException()
        {
            const string productsServiceName = "products";

            var entries = new Dictionary<string, EndpointEntry>();
            entries.Add(productsServiceName, new EndpointEntry());

            var serviceEntry = new ServiceEntry { Name = productsServiceName };
            var clientEntry = new ClientEntry();
            var endpoint = new EndpointEntry();
            var policies = new HttpPoliciesEntry();

            Action action = () => EndpointEntriesContext.AddEndpointEntry(entries.ToImmutableDictionary(), serviceEntry,
                clientEntry, endpoint, policies);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'Name')", exception.Message);
        }

        [Fact]
        public void AddEndpointEntry_NullEndpointPath_ShouldThrowException()
        {
            const string productsServiceName = "products";

            var entries = new Dictionary<string, EndpointEntry>();
            entries.Add(productsServiceName, new EndpointEntry());

            var serviceEntry = new ServiceEntry { Name = productsServiceName };
            var clientEntry = new ClientEntry();
            var endpoint = new EndpointEntry { Name = "test" };
            var policies = new HttpPoliciesEntry();

            Action action = () => EndpointEntriesContext.AddEndpointEntry(entries.ToImmutableDictionary(), serviceEntry,
                clientEntry, endpoint, policies);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null. (Parameter 'Path')", exception.Message);
        }

        [Fact]
        public void AddEndpointEntry_DuplicatedEndpoint_ShouldThrowException()
        {
            const string productsServiceName = "products";
            const string endpointName = "endpoint";

            var entries = new Dictionary<string, EndpointEntry>();
            entries.Add(endpointName, new EndpointEntry { Name = endpointName });

            var serviceEntry = new ServiceEntry { Name = productsServiceName };
            var clientEntry = new ClientEntry();
            var endpoint = new EndpointEntry { Name = endpointName, Path = "path" };
            var policies = new HttpPoliciesEntry();

            Action action = () => EndpointEntriesContext.AddEndpointEntry(entries.ToImmutableDictionary(), serviceEntry,
                clientEntry, endpoint, policies);

            var exception = Assert.Throws<EndpointEntryAlreadyDefinedException>(action);
            var message = EndpointEntryAlreadyDefinedException.CreateMessage(productsServiceName, endpointName);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void AddEndpointEntry_DuplicatedEndpointPolicy_ShouldThrowException()
        {
            const string productsServiceName = "products";
            const string endpointName = "endpoint";

            var entries = new Dictionary<string, EndpointEntry>();
            entries.Add(endpointName, new EndpointEntry { Name = endpointName });

            const string policyName = "policy_name";
            const string wrongPolicy = "wrong_policy";

            var serviceEntry = new ServiceEntry { Name = productsServiceName };
            var clientEntry = new ClientEntry();
            var endpoint = new EndpointEntry
            {
                Name = "test",
                Path = "path",
                PolicyName = wrongPolicy
            };

            var policies = new HttpPoliciesEntry();
            policies.Entries.Add(policyName, new HttpPoliciesItem());

            Action action = () => EndpointEntriesContext.AddEndpointEntry(entries.ToImmutableDictionary(), serviceEntry,
                clientEntry, endpoint, policies);

            var exception = Assert.Throws<PolicyNotDefinedException>(action);
            var message = PolicyNotDefinedException.CreateMessage(wrongPolicy);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void AddEndpointEntry_TestBind()
        {
            const string productsServiceName = "products";
            const string endpointName = "endpoint";

            var entries = new Dictionary<string, EndpointEntry>();
            entries.Add(endpointName, new EndpointEntry { Name = endpointName });

            const string policyName = "policy_name";

            var serviceEntry = new ServiceEntry { Name = productsServiceName };
            var clientEntry = new ClientEntry();
            var endpoint = new EndpointEntry
            {
                Name = "test",
                Path = "path",
                PolicyName = policyName
            };

            var policies = new HttpPoliciesEntry();
            policies.Entries.Add(policyName, new HttpPoliciesItem());

            var entry = EndpointEntriesContext.AddEndpointEntry(entries.ToImmutableDictionary(), serviceEntry,
                clientEntry, endpoint, policies);
            Assert.Single(entries);
            Assert.Equal(2, entry.Count);
            //Assert all properties
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithRetryFilled()
        {
            const int retries = 6;
            const int firstRetry = 2;
            const int progression = 12;

            var entry = new EndpointEntry
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = retries,
                    FirstRetryWaitTimeInSeconds = firstRetry,
                    ProgressionType = TimeProgressionType.None,
                    RetryProgressionBaseValueInSeconds = progression
                }
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, null);

            Assert.Equal(retries, entry.Retry.NumberOfRetries);
            Assert.Equal(firstRetry, entry.Retry.FirstRetryWaitTimeInSeconds);
            Assert.Equal(progression, entry.Retry.RetryProgressionBaseValueInSeconds);
            Assert.Equal(TimeProgressionType.None, entry.Retry.ProgressionType);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithNullPolicyRetry()
        {
            var entry = new EndpointEntry
            {
                Retry = null
            };

            var httpPolicy = new HttpPoliciesItem
            {
                Retry = null
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, httpPolicy);
            Assert.Null(entry.Retry);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithPolicyRetry()
        {
            const int retries = 6;
            const int firstRetry = 2;
            const int progression = 12;

            var entry = new EndpointEntry
            {
                Retry = null
            };

            var httpPolicy = new HttpPoliciesItem
            {
                Retry = new RetryPolicyEntry
                {
                    NumberOfRetries = retries,
                    FirstRetryWaitTimeInSeconds = firstRetry,
                    ProgressionType = TimeProgressionType.None,
                    RetryProgressionBaseValueInSeconds = progression
                }
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, httpPolicy);
            Assert.Equal(retries, entry.Retry.NumberOfRetries);
            Assert.Equal(firstRetry, entry.Retry.FirstRetryWaitTimeInSeconds);
            Assert.Equal(progression, entry.Retry.RetryProgressionBaseValueInSeconds);
            Assert.Equal(TimeProgressionType.None, entry.Retry.ProgressionType);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithCircuitFilled()
        {
            const int duration = 6;
            const int events = 2;

            var entry = new EndpointEntry
            {
                CircuitBreak = new CircuitBreakPolicyEntry
                {
                    DurationOfBreakInSeconds = duration,
                    EventsTimesBeforeBreaking = events
                }
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, null);

            Assert.Equal(duration, entry.CircuitBreak.DurationOfBreakInSeconds);
            Assert.Equal(events, entry.CircuitBreak.EventsTimesBeforeBreaking);
            Assert.Null(entry.Retry);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithNullCircuit()
        {
            var entry = new EndpointEntry
            {
                Retry = null,
                CircuitBreak = null
            };

            var httpPolicy = new HttpPoliciesItem
            {
                Retry = null,
                CircuitBreak = null
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, httpPolicy);
            Assert.Null(entry.Retry);
            Assert.Null(entry.CircuitBreak);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_WithPolicyCircuit()
        {
            const int duration = 6;
            const int events = 2;

            var entry = new EndpointEntry
            {
                CircuitBreak = null
            };

            var httpPolicy = new HttpPoliciesItem
            {
                CircuitBreak = new CircuitBreakPolicyEntry
                {
                    DurationOfBreakInSeconds = duration,
                    EventsTimesBeforeBreaking = events
                }
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, httpPolicy);

            Assert.Equal(duration, entry.CircuitBreak.DurationOfBreakInSeconds);
            Assert.Equal(events, entry.CircuitBreak.EventsTimesBeforeBreaking);
            Assert.Null(entry.Retry);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_Endpoint_Timeout()
        {
            const int timeout = 60;

            var entry = new EndpointEntry
            {
                Retry = null,
                CircuitBreak = null,
                TimeoutInSeconds = timeout
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, null);
            Assert.Equal(timeout, entry.TimeoutInSeconds);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_Endpoint_DefaulTimeout()
        {
            var entry = new EndpointEntry
            {
                Retry = null,
                CircuitBreak = null,
                TimeoutInSeconds = 0
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, null);
            Assert.Equal(EndpointEntriesContext.DefaultTimeout, entry.TimeoutInSeconds);
        }

        [Fact]
        public void UpdateEndpointEntryPolicies_Endpoint_PolicyTimeout()
        {
            const int timeout = 160;

            var entry = new EndpointEntry
            {
                Retry = null,
                CircuitBreak = null,
                TimeoutInSeconds = 0
            };

            var httpPolicy = new HttpPoliciesItem
            {
                TimeoutInSeconds = timeout
            };

            EndpointEntriesContext.UpdateEndpointEntryPolicies(entry, httpPolicy);
            Assert.Equal(timeout, entry.TimeoutInSeconds);
        }
    }
}