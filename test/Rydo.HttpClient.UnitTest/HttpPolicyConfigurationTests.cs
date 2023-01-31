namespace Rydo.HttpClient.UnitTest
{
    using Configurations;
    using Xunit;

    public class HttpPolicyConfigurationTests
    {
        [Fact]
        public void HttpPolicyConfiguration_TestBind_EmptyPolicies()
        {
            var policyConfiguration = new HttpPolicyConfiguration();
            Assert.NotNull(policyConfiguration);
            Assert.Empty(policyConfiguration.Policies);
        }

        [Fact]
        public void HttpPolicyConfiguration_TestBind_Policies()
        {
            var policyConfiguration = new HttpPolicyConfiguration();
            policyConfiguration.Policies.Add("Test1", new PolicyConfiguration
            {
                FirstRetryWaitTimeInSeconds = 1,
                NumberOfRetries = 1,
                ProgressionType = TimeProgressionType.None,
                RetryProgressionBaseValueInSeconds = 1
            });

            Assert.NotNull(policyConfiguration);
            Assert.Single(policyConfiguration.Policies);
        }
    }
}