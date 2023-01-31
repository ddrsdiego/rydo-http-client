namespace Rydo.HttpClient.Configurations
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpPoliciesEntryEx
    {
        public static HttpPoliciesEntry TryGetHttpPoliciesEntry(this IServiceCollection services,
            IConfiguration configuration)
        {
            var httpPoliciesEntry = new HttpPoliciesEntry();

            var section = configuration.GetSection(Constants.HttpPoliciesEntrySection);
            section.Bind(httpPoliciesEntry.Entries);

            return httpPoliciesEntry;
        }
    }
}